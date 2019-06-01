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

        const string UserAccountCreatedTopic = "UserAccountCreatedTopic";

        static string SubscriptionName
        {
            get
            {
                return Environment.MachineName;
            }
        }

        static long _messageProcessedCounter = 0;
        static long _messageProcessedCounterPrevious = -1;

        private static void DisplayMessageProcessed()
        {
            if(_messageProcessedCounter != _messageProcessedCounterPrevious)
            {
                if(_messageProcessedCounterPrevious != -1)
                {
                    var messageProcessedInTheLastSecond = _messageProcessedCounter - _messageProcessedCounterPrevious;
                    Console.WriteLine($"{_messageProcessedCounter} Message Processed Total, {messageProcessedInTheLastSecond} / S");
                }
                else
                {
                    Console.WriteLine($"{_messageProcessedCounter} Message Processed Total");
                }
                
                _messageProcessedCounterPrevious = _messageProcessedCounter;
            }
        }

        static bool OnMessageReceived(string messageBody, string messageId, long sequenceNumber)
        {
            _messageProcessedCounter++;
            // Console.WriteLine($">>> [{messageId}, {sequenceNumber}] {messageBody}");
            return true;
        }

        static AzurePubSubManager sub = null;

        static async Task Subscribe()
        {
            sub = new AzurePubSubManager(AzurePubSubManagerType.Subcribe, GetServiceBusConnectionString(), UserAccountCreatedTopic, SubscriptionName);
            sub.Subscribe(OnMessageReceived);
            Console.WriteLine("Waiting for messages");

            if (RuntimeHelper.IsRunningContainerMode())
            {
                AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
                while(true)
                {
                    Thread.Sleep(1000);
                    DisplayMessageProcessed();
                }
            }
            else
            {
                Console.WriteLine("Q)uit C)ls");
                while (true)
                {
                    if (Console.KeyAvailable)
                    {
                        var k = Console.ReadKey();
                        if (k.Key == ConsoleKey.Q)
                            break;
                        if (k.Key == ConsoleKey.C)
                            Console.Clear();
                        Console.WriteLine("Q)uit C)ls");
                    }
                    Thread.Sleep(1000);
                    DisplayMessageProcessed();
                }
                await sub.Close();
            }
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("CurrentDomain_ProcessExit");
            sub.StopSubscribingAsync().GetAwaiter().GetResult();
        }
    }
}
