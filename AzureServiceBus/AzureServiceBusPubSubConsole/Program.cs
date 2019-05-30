using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace AzureServiceBusPubSubConsole
{
    class Program
    {
        static void Help()
        {
            Console.WriteLine(@"
AzureServiceBusPubSubConsole.exe publish | subscribe
");
        }
        static void Main(string[] args)
        {
            if(args.Length != 1)
            {
                Help();
            }
            else
            {
                switch(args[0].ToLowerInvariant())
                {
                    case "publish": Publish().GetAwaiter().GetResult(); break;
                    case "subscribe": Subscribe().GetAwaiter().GetResult(); break;
                    default: Help(); break;
                }
            }
        }

        const string ServiceBusConnectionString = "Endpoint=sb://fmicroservices.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=/fit7G9QSeG/1yutghI=";
        const string TopicName = "myTopic";
        static ITopicClient topicClient;

        const string SubscriptionName = "S1";
        static ISubscriptionClient subscriptionClient;

        static bool OnMessageReceived(string messageBody, string messageId, long sequenceNumber)
        {
            Console.WriteLine($">>> [{messageId}, {sequenceNumber}] {messageBody}");
            return true;
        }

        static async Task Subscribe()
        {
            var sub = new AzurePubSubManager(AzurePubSubManagerType.Subcribe, ServiceBusConnectionString, TopicName, SubscriptionName);

            Console.WriteLine("======================================================");
            Console.WriteLine("Press ENTER key to exit after receiving all the messages.");
            Console.WriteLine("======================================================");

            sub.Subscribe(OnMessageReceived);

            Console.ReadKey();

            await sub.StopSubscribingAsync();
        }

        static async Task Publish()
        {
            var pub = new AzurePubSubManager(AzurePubSubManagerType.Publish, ServiceBusConnectionString, TopicName);
            await pub.PublishAsync("Hello 1");
            await pub.PublishAsync("Hello 2");
        }


    }
}
