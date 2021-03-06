﻿using AzureServiceBusSubHelper;
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

        public SystemActivityNotificationManager(string serviceBusConnectionString, string subscriptionName)
        {
            _pubSub = new AzurePubSubManager(AzurePubSubManagerType.Subcribe, serviceBusConnectionString, SystemActivityTopic, subscriptionName);
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

                return true;
            }
        }

        public SystemActivityNotificationManager(string serviceBusConnectionString)
        {
            _pubSub = new AzurePubSubManager(AzurePubSubManagerType.Publish, serviceBusConnectionString, SystemActivityTopic);            
        }

        public async Task CloseAsync()
        {
            await this._pubSub.CloseAsync();
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

        public async Task NotifyErrorAsync(Errors errors, bool sendToConsole = true)
        {
            foreach(var error in errors)
                await this.NotifyErrorAsync(error.Reason,error.Exception, sendToConsole);
        }

        public async Task NotifyErrorAsync(string message, bool sendToConsole = true)
        {
            await this.NotifyAsync(message, SystemActivityType.Error, sendToConsole);
        }

        public async Task NotifyErrorAsync(string message, System.Exception ex, bool sendToConsole = true)
        {
            await this.NotifyAsync($"{message} - ex:{ex}", SystemActivityType.Error, sendToConsole);
        }

        public async Task NotifyInfoAsync(string message, Dictionary<string, object> properties, bool sendToConsole = true)
        {
            var propertiesContent = new StringBuilder();
            foreach (var e in properties)
                propertiesContent.Append($"{e.Key}:{e.Value}; ");
            await NotifyAsync($"{message} [{propertiesContent}]", SystemActivityType.Info, sendToConsole);
        }

        public async Task NotifyInfoAsync(string message, bool sendToConsole = true)
        {
            await NotifyAsync(message, SystemActivityType.Info, sendToConsole);
        }

        public async Task NotifyAsync(string message, SystemActivityType type = SystemActivityType.Info, bool sendToConsole = true)
        {
            await NotifyAsync(new SystemActivity(message, type), sendToConsole);
        }

        public async Task NotifyAsync(SystemActivity sa, bool sendToConsole = true)
        {
            await _pubSub.PublishAsync(sa.ToJSON());
            if (sendToConsole)
                System.Console.WriteLine($"[san:{sa.Type}, {sa.MachineName}, {sa.UtcDateTime}]{sa.Message}");
        }
    }
}
