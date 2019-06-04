﻿using AzureServiceBusSubHelper;
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
        /// Parameter -generationIndex is passed as an envrironment variable when running
        /// as a container
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            System.Console.WriteLine(RuntimeHelper.GetContextInformation());

            Monitor(1).GetAwaiter().GetResult();
        }

        static string GetServiceBusConnectionString()
        {
            return RuntimeHelper.GetAppSettings("connectionString:ServiceBusConnectionString");
        }

        private static void SystemActivityNotificationSubscriber_OnMessageReveived(SystemActivity sa)
        {
            ConsoleEx.WriteLineAutoColor($"[{sa.Type}] Host:{sa.MachineName}, {sa.UtcDateTime.ToShortTimeString()}, {sa.Message}", sa.MachineName);
        }

        static async Task Monitor(int generationIndex)
        {
            var systemActivityNotificationSubscriber = new SystemActivityNotificationManager(GetServiceBusConnectionString(), Environment.MachineName);
            systemActivityNotificationSubscriber.OnMessageReveived += SystemActivityNotificationSubscriber_OnMessageReveived;
            
            int previousQueueCount = -1;
            try
            {
                var donationQueue = new DonationQueue(RuntimeHelper.GetAppSettings("storage:AccountName"), RuntimeHelper.GetAppSettings("storage:AccountKey"));
                var donationTableManager = new DonationTableManager(RuntimeHelper.GetAppSettings("storage:AccountName"), RuntimeHelper.GetAppSettings("storage:AccountKey"));
                System.Console.Title = $"Donation.Monitor.Console Q)uit C)ls";
                var goOn = true;
                while (goOn)
                {
                    Thread.Sleep(4 * 1000);
                    if(System.Console.KeyAvailable)
                    {
                        switch(System.Console.ReadKey().Key)
                        {
                            case ConsoleKey.Q: goOn = false;  break;
                            case ConsoleKey.C: System.Console.Clear(); break;
                        }
                    }
                    var queueCount = await donationQueue.ApproximateMessageCountAsync();
                    if(previousQueueCount != queueCount)
                    {
                        System.Console.WriteLine($"{queueCount} Message in Queue {donationQueue.QueueName}");
                        previousQueueCount = queueCount;
                    }
                }                
            }
            catch(System.Exception ex)
            {
                System.Console.WriteLine(ex);
            }
        }


    }
}