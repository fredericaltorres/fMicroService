using System;
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
            Publish().GetAwaiter().GetResult();
        }

        static string GetServiceBusConnectionString()
        {
            var s = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
            if(string.IsNullOrEmpty(s))
                s = RuntimeHelper.GetAppSettings("connectionString:ServiceBusConnectionString");
            return s;
        }

        const string TopicName = "myTopic";

        static async Task Publish()
        {
            while (true)
            {
                var pub = new AzurePubSubManager(AzurePubSubManagerType.Publish, GetServiceBusConnectionString(), TopicName);
                for (var i = 0; i < 100; i++)
                {
                    await pub.PublishAsync($"Hello {i} - {DateTime.Now}");
                }
                Thread.Sleep(30 * 1000);
                //Console.WriteLine("Q)uit C)ontinue");
                //var k = Console.ReadKey();
                //if (k.Key == ConsoleKey.Q)
                //    break;
            }
        }
    }
}
