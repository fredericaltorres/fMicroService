using AzureServiceBusSubHelper;
using Donation.Model;
using Donation.Model.Lib.Util;
using Donation.Queue.Lib;
using Donation.Service;
using Donation.Table.Lib;
using DynamicSugar;
using fAzureHelper;
using fDotNetCoreContainerHelper;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Donation.PersonSimulator.Console
{
    class Program
    {
        /// <summary>
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            System.Console.WriteLine(RuntimeHelper.GetContextInformation());
            if(RuntimeHelper.GetCommandLineParameterBool("-deleteTable", args))
            {
                GetDonationTableManager().DeleteAsync().GetAwaiter().GetResult();
                GetDonationAggregateTableManager().DeleteAsync().GetAwaiter().GetResult();
                System.Console.WriteLine("Tables deleted");
            }
            else Monitor().GetAwaiter().GetResult();
        }

        static string GetServiceBusConnectionString()
        {
            return RuntimeHelper.GetAppSettings("connectionString:ServiceBusConnectionString");
        }

        private static void SystemActivityNotificationSubscriber_OnMessageReveived(SystemActivity sa)
        {
            ConsoleEx.WriteLineAutoColor($"[{sa.Type}] Host:{sa.MachineName}\r\n      {sa.UtcDateTime.ToShortTimeString()}, {sa.Message}", sa.MachineName);
        }

        static void Wait(int waitMaxSecond)
        {
            for(var i=0; i< waitMaxSecond; i++)
            {
                if (System.Console.KeyAvailable)
                    break;
                Thread.Sleep(1000);
            }
        }

        static async Task Monitor()
        {
            var systemActivityNotificationSubscriber = new SystemActivityNotificationManager(GetServiceBusConnectionString(), Environment.MachineName);
            systemActivityNotificationSubscriber.OnMessageReceived += SystemActivityNotificationSubscriber_OnMessageReveived;
            const int waitSeconds = 4;
            int previousQueueCount = -1;
            try
            {
                var donationQueue = new DonationQueue(RuntimeHelper.GetAppSettings("storage:AccountName"), RuntimeHelper.GetAppSettings("storage:AccountKey"));
                GetDonationTableManager();
                System.Console.Title = $"Donation.Monitor.Console Q)uit C)ls P)ause";
                var goOn = true;
                // var pausedMode = false;
                while (goOn)
                {
                    Wait(waitSeconds);
                    if (System.Console.KeyAvailable)
                    {
                        switch (System.Console.ReadKey(true).Key)
                        {
                            case ConsoleKey.Q: goOn = false; break;
                            case ConsoleKey.C: System.Console.Clear(); break;
                            case ConsoleKey.P:
                                {
                                    systemActivityNotificationSubscriber.PauseOnMessageReceived = true;
                                    System.Console.WriteLine("Monitoring paused - hit any key to continue");
                                    System.Console.ReadKey();
                                    systemActivityNotificationSubscriber.PauseOnMessageReceived = false;
                                }
                                break;
                        }
                    }
                    var queueCount = await donationQueue.ApproximateMessageCountAsync();
                    if (previousQueueCount != queueCount)
                    {
                        System.Console.Write($"{queueCount} messages in queue {donationQueue.QueueName}");
                        if (previousQueueCount > queueCount)
                        {
                            var msgProcessedInInterval = previousQueueCount - queueCount;
                            var msgProcessedPerSecond = 1.0 * msgProcessedInInterval / waitSeconds;
                            System.Console.Write($" {msgProcessedInInterval} donations processed in {waitSeconds}, {msgProcessedPerSecond} donations processed per second (From queue)");
                        }
                        System.Console.WriteLine("");
                    }
                    previousQueueCount = queueCount;
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex);
            }
        }

        private static DonationTableManager GetDonationTableManager()
        {
            return new DonationTableManager(RuntimeHelper.GetAppSettings("storage:AccountName"), RuntimeHelper.GetAppSettings("storage:AccountKey"));
        }
        private static DonationAggregateTableManager GetDonationAggregateTableManager()
        {
            return new DonationAggregateTableManager(RuntimeHelper.GetAppSettings("storage:AccountName"), RuntimeHelper.GetAppSettings("storage:AccountKey"));
        }
    }
}
