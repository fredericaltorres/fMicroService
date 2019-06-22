using AzureServiceBusSubHelper;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace fAzureHelper
{
    public class SystemActivityNotificationManager
    {
        public const string SystemActivityTopic = "systemactivity";
        public const int NotifyEvery = 500;

        public delegate void OnMessageReceivedEventHandler(SystemActivity systemActivity);
        public event OnMessageReceivedEventHandler OnMessageReceived;

        public bool PauseOnMessageReceived = false;

        private AzurePubSubManager _pubSub;

        public SystemActivityNotificationManager(string serviceBusConnectionString, string subscriptionName, bool purgeSubscription)
        {
            _pubSub = new AzurePubSubManager(AzurePubSubManagerType.Subcribe, serviceBusConnectionString, SystemActivityTopic, subscriptionName, purgeSubscription: purgeSubscription);

            _pubSub.Subscribe(InternalOnMessageReceived);
        }

        private bool InternalOnMessageReceived(string messageBody, string messageId, long sequenceNumber)
        {
            var sa = SystemActivity.FromJson(messageBody);
            if (sa == null)
            {
                return true; // TODO: For now ignore the issue
            }
            else
            {
                if (OnMessageReceived != null && !PauseOnMessageReceived)
                    OnMessageReceived(sa);

                // System.Console.WriteLine($"[{sa.Type}] Host:{sa.MachineName}, {sa.UtcDateTime}, {sa.Message}");
                return true;
            }
        }

        public SystemActivityNotificationManager(string serviceBusConnectionString)
        {
            _pubSub = new AzurePubSubManager(AzurePubSubManagerType.Publish, serviceBusConnectionString, SystemActivityTopic);
        }

        public async Task<string> NotifyPerformanceInfoAsync(SystemActivityPerformanceType performanceType, string action, int durationSecond, int itemProcessedPerSeconds, long totalItemProcessed, bool sendToConsole = true)
        {
            var sa = new SystemActivity().SetPerformanceInfo(performanceType, action, durationSecond, itemProcessedPerSeconds, totalItemProcessed);
            await NotifyAsync(sa);
            if (sendToConsole)
                System.Console.WriteLine($"[san:{sa.Type}]{sa.Message}");
            return sa.Message;
        }

        public async Task<string> NotifySetDashboardInfoInfoAsync(string dashboardResource, string jsonData, int totalItemProcessed, bool sendToConsole = true)
        {
            var sa = new SystemActivity().SetDashboardInfo(dashboardResource, jsonData, totalItemProcessed);
            await NotifyAsync(sa);
            if (sendToConsole)
                System.Console.WriteLine($"[san:{sa.Type}]{sa.Message}");
            return sa.Message;
        }

        public async Task NotifyAsync(List<string> messages, SystemActivityType type = SystemActivityType.Info, bool sendToConsole = true)
        {
            foreach (var message in messages)
                await this.NotifyAsync(message, type, sendToConsole);
        }

        //public string Notify(string message, SystemActivityType type = SystemActivityType.Info, bool sendToConsole = true)
        //{
        //    // Wait for the call so the notification are logged in the right order
        //    return NotifyAsync(message, type, sendToConsole).GetAwaiter().GetResult();
        //}

        public async Task<string> NotifyAsync(string message, SystemActivityType type = SystemActivityType.Info, bool sendToConsole = true)
        {
            var systemActivity = new SystemActivity(message, type);
            await NotifyAsync(systemActivity);
            if (sendToConsole)
                System.Console.WriteLine($"[san:{type}]{message}");
            return message;
        }

        public async Task NotifyAsync(SystemActivity sa)
        {
            await _pubSub.PublishAsync(sa.ToJSON());
        }
    }
}
