﻿using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureServiceBusPubSubConsole
{
    public enum AzurePubSubManagerType
    {
        Publish,
        Subcribe
    };
    public class AzurePubSubManager
    {
        AzurePubSubManagerType _type;
        string _connectionString;
        string _topic;
        string _subscriptionName;

        static ITopicClient topicClient;

        static ISubscriptionClient subscriptionClient;

        public delegate bool OnMessageReceived(string messageBody, string messageId, long sequenceNumber);

        OnMessageReceived _onMessageReceived;

        public AzurePubSubManager(AzurePubSubManagerType type, string connectionString, string topic, string subscriptionName = null)
        {
            _type = type;
            _connectionString = connectionString;
            _topic = topic;
            _subscriptionName = subscriptionName;

            if(type == AzurePubSubManagerType.Publish)
            {
                topicClient = new TopicClient(_connectionString, _topic);
            }
            else if(type ==AzurePubSubManagerType.Subcribe)
            {
                subscriptionClient = new SubscriptionClient(_connectionString, _topic, _subscriptionName);
            }
        }

        public async Task PublishAsync(string messageBody)
        {
            var message = new Message(Encoding.UTF8.GetBytes(messageBody));
            Console.WriteLine($"Publishing {messageBody}, messageId:{message.MessageId}");            
            await topicClient.SendAsync(message);
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
            subscriptionClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        public async Task StopSubscribingAsync()
        {
            await subscriptionClient.CloseAsync();
        }


        async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            // Process the message.
            var messageBody = Encoding.UTF8.GetString(message.Body);
            var sequenceNumber = message.SystemProperties.SequenceNumber;
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{messageBody}");

            var r = _onMessageReceived(messageBody, message.MessageId, sequenceNumber);
            if(r)
            {
                // Complete the message so that it is not received again.
                // This can be done only if the subscriptionClient is created in ReceiveMode.PeekLock mode (which is the default).
                await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
            }
            else
            {
                // Release message so it can be processed again
                await subscriptionClient.AbandonAsync(message.SystemProperties.LockToken);
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
