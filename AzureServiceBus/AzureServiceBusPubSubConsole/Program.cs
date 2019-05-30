using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using fDotNetCoreContainerHelper;
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

        static string GetServiceBusConnectionString()
        {
            var s = RuntimeHelper.GetAppSettings("connectionString:ServiceBusConnectionString");
            return s;
        }

        const string TopicName = "myTopic";
        const string SubscriptionName = "S3";

        static bool OnMessageReceived(string messageBody, string messageId, long sequenceNumber)
        {
            Console.WriteLine($">>> [{messageId}, {sequenceNumber}] {messageBody}");
            return true;
        }

        static async Task Subscribe()
        {
            var sub = new AzurePubSubManager(AzurePubSubManagerType.Subcribe, GetServiceBusConnectionString(), TopicName, SubscriptionName);

            Console.WriteLine("======================================================");
            Console.WriteLine("Press ENTER key to exit after receiving all the messages.");
            Console.WriteLine("======================================================");

            sub.Subscribe(OnMessageReceived);

            Console.ReadKey();

            await sub.StopSubscribingAsync();
        }

        static async Task Publish()
        {
            var pub = new AzurePubSubManager(AzurePubSubManagerType.Publish, GetServiceBusConnectionString(), TopicName);
            await pub.PublishAsync("Hello 1");
            await pub.PublishAsync("Hello 2");
        }


    }
}
