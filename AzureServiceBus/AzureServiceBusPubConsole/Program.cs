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
            Console.WriteLine(RuntimeHelper.GetContextInformation());
            Publish().GetAwaiter().GetResult();
            Console.WriteLine("Job done waiting for ever");
            while (true)
            {
                Thread.Sleep(10 * 1000);
            }
        }

        static string GetServiceBusConnectionString()
        {
            var s = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
            if(string.IsNullOrEmpty(s))
                s = RuntimeHelper.GetAppSettings("connectionString:ServiceBusConnectionString");
            return s;
        }

        const string UserAccountCreatedTopic = "UserAccountCreatedTopic";

        static async Task Publish()
        {
            const int maxMessageToSend = 1000;
            var pub = new AzurePubSubManager(AzurePubSubManagerType.Publish, GetServiceBusConnectionString(), UserAccountCreatedTopic);
            for (var i = 0; i < maxMessageToSend; i++)
            {
                await pub.PublishAsync($"Hello {i} - {DateTime.Now}");
            }
            Console.WriteLine($"{maxMessageToSend} {UserAccountCreatedTopic} messages published");
        }
    }
}
