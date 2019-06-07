using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureServiceBusSubHelper
{
    public enum AzurePubSubManagerType
    {
        Publish,
        Subcribe
    };
    public class AzurePubSubManager
    {
        const int MAX_SUBSCRIPTION_NAME_LENGTH = 50;

        private AzurePubSubManagerType _type;
        private string _connectionString;
        private string _topic;
        private string _subscriptionName;
        private ITopicClient _topicClient;
        private ISubscriptionClient _subscriptionClient;
        private bool _mustCreateDeleteSubscription;

        private OnMessageReceived _onMessageReceived;

        public long MessagePublishedCounter = 0;
        public DateTime MessagePublishedTimeStamp;

        public long MessageReceivedCounter = 0;
        public DateTime MessageReceivedTimeStamp;

        public delegate bool OnMessageReceived(string messageBody, string messageId, long sequenceNumber);

        public AzurePubSubManager(AzurePubSubManagerType type, string connectionString, string topic, string subscriptionName = null, bool mustCreateSubscription = true)
        {
            _type = type;
            _connectionString = connectionString;
            _topic = topic;

            _mustCreateDeleteSubscription = mustCreateSubscription;

            if (type == AzurePubSubManagerType.Publish)
            {
                _topicClient = new TopicClient(_connectionString, _topic);
            }
            else if(type ==AzurePubSubManagerType.Subcribe)
            {
                _subscriptionName = subscriptionName.Substring(0, Math.Min(MAX_SUBSCRIPTION_NAME_LENGTH, subscriptionName.Length));
                if (mustCreateSubscription)
                {
                    this.CreateSubscriptionIfNeededAsync(_subscriptionName).GetAwaiter().GetResult();
                }
                _subscriptionClient = new SubscriptionClient(_connectionString, _topic, _subscriptionName);
            }
        }

        private async Task CreateSubscriptionIfNeededAsync(string subscriptionName)
        {
            var managementClient = new ManagementClient(_connectionString);
            if(!await managementClient.SubscriptionExistsAsync(_topic, subscriptionName))
            {
                var subscriptionDescription = await managementClient.CreateSubscriptionAsync(_topic, subscriptionName);
            }
        }

        public async Task Close()
        {
            if (_type == AzurePubSubManagerType.Subcribe) {
                await this.StopSubscribingAsync();
                if (_mustCreateDeleteSubscription)
                {
                    var managementClient = new ManagementClient(_connectionString);
                    await managementClient.DeleteSubscriptionAsync(_topic, _subscriptionName);
                }
            }
        }

        public string GetReceivedInformation()
        {
            var duration = (DateTime.UtcNow - MessageReceivedTimeStamp).TotalSeconds;
            var messagePerSecond = MessageReceivedCounter / duration;
            return $"{MessageReceivedCounter} messages received {duration:0.0} seconds, {messagePerSecond:0.0} message/S";
        }

        public string GetSendInformation()
        {
            var duration = (DateTime.UtcNow - MessagePublishedTimeStamp).TotalSeconds;
            var messagePerSecond = MessagePublishedCounter / duration;
            return $"{MessagePublishedCounter} messages published {duration:0.0} seconds, {messagePerSecond:0.0} message/S";
        }

        public async Task PublishAsync(string messageBody, string messageId = null)
        {
            if(MessagePublishedCounter == 0) // Set MessageSentTimeStamp on the first message that we send
                MessagePublishedTimeStamp = DateTime.UtcNow;

            MessagePublishedCounter++;

            var message = new Message(Encoding.UTF8.GetBytes(messageBody));
            if (messageId == null)
                messageId = Guid.NewGuid().ToString();
            message.MessageId = messageId;
            await _topicClient.SendAsync(message);
        }

        public void Subscribe(OnMessageReceived onMessageReceived)
        {
            _onMessageReceived = onMessageReceived;

            // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,
                
                // Indicates whether the message pump should automatically complete the messages after returning from user callback.
                // False below indicates the complete operation is handled by the user callback as in ProcessMessagesAsync().
                AutoComplete = false
            };
            
            // Register the function that processes messages.
            _subscriptionClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        public async Task StopSubscribingAsync()
        {
            await _subscriptionClient.CloseAsync();
        }

        async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            // Process the message.
            var messageBody = Encoding.UTF8.GetString(message.Body);
            var sequenceNumber = message.SystemProperties.SequenceNumber;
            var messageId = message.MessageId;

            if (MessageReceivedCounter == 0) // Set MessageSentTimeStamp on the first message that we send
                MessageReceivedTimeStamp = DateTime.UtcNow;

            MessageReceivedCounter++;

            // Console.WriteLine($"Received message: MessageId:{messageId}, SequenceNumber:{sequenceNumber} Body:{messageBody}");

            var r = _onMessageReceived(messageBody, message.MessageId, sequenceNumber);
            if(r)
            {
                // Complete the message so that it is not received again.
                // This can be done only if the subscriptionClient is created in ReceiveMode.PeekLock mode (which is the default).
                await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);                
            }
            else
            {
                // Release message so it can be processed again
                await _subscriptionClient.AbandonAsync(message.SystemProperties.LockToken);
            }
            // Note: Use the cancellationToken passed as necessary to determine if the subscriptionClient has already been closed.
            // If subscriptionClient has already been closed, you can choose to not call CompleteAsync() or AbandonAsync() etc.
            // to avoid unnecessary exceptions.
        }

        // Use this handler to examine the exceptions received on the message pump.
        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
    }
}
