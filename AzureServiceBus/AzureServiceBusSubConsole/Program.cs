using System;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureServiceBusSubHelper;
using fDotNetCoreContainerHelper;
using Microsoft.Azure.ServiceBus;

namespace AzureServiceBusPubSubConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(RuntimeHelper.GetContextInformation());
            Subscribe().GetAwaiter().GetResult();
        }

        static string GetServiceBusConnectionString()
        {
            var s = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
            if(string.IsNullOrEmpty(s))
                s = RuntimeHelper.GetAppSettings("connectionString:ServiceBusConnectionString");
            return s;
        }

        const string TopicName = "myTopic";
        const string SubscriptionName = "S1";

        static bool OnMessageReceived(string messageBody, string messageId, long sequenceNumber)
        {
            Console.WriteLine($">>> [{messageId}, {sequenceNumber}] {messageBody}");
            return true;
        }

        static AzurePubSubManager sub = null;

        static async Task Subscribe()
        {
            sub = new AzurePubSubManager(AzurePubSubManagerType.Subcribe, GetServiceBusConnectionString(), TopicName, SubscriptionName);
            sub.Subscribe(OnMessageReceived);
            Console.WriteLine("Waiting for messages");

            if (RuntimeHelper.IsRunningContainerMode())
            {
                AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
                while(true)
                {
                    Thread.Sleep(500);
                }
            }
            else
            {
                WaitOnWindows();
                await sub.StopSubscribingAsync();
            }
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("CurrentDomain_ProcessExit");
            sub.StopSubscribingAsync().GetAwaiter().GetResult();
        }

        private static void WaitOnWindows()
        {
            while (true)
            {
                Console.WriteLine("Q)uit C)ls");
                var k = Console.ReadKey();
                if (k.Key == ConsoleKey.Q)
                    break;
                if (k.Key == ConsoleKey.C)
                    Console.Clear();
            }
        }
    }
}
