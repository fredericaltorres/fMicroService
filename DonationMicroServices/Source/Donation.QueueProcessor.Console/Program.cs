using AzureServiceBusSubHelper;
using Donation.Model;
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
            ProcessDonationQueue(1).GetAwaiter().GetResult();
        }

        static string GetServiceBusConnectionString()
        {
            return RuntimeHelper.GetAppSettings("connectionString:ServiceBusConnectionString");
        }

        static async Task ProcessDonationQueue(int generationIndex)
        {
            var saNotification = new SystemActivityNotificationManager(GetServiceBusConnectionString());
            saNotification.Notify($"Donation.QueueProcessor.Console Running");
            try
            {
                // Settings come frm the appsettings.json file
                var donationQueue = new DonationQueue(RuntimeHelper.GetAppSettings("storage:AccountName"), RuntimeHelper.GetAppSettings("storage:AccountKey"));
                var donationTableManager = new DonationTableManager(RuntimeHelper.GetAppSettings("storage:AccountName"), RuntimeHelper.GetAppSettings("storage:AccountKey"));

                while (true)
                {
                    (DonationDTO donationDTO, string messageId) = await donationQueue.DequeueAsync();
                    if (donationDTO == null)
                    {
                        Thread.Sleep(2 * 1000);
                    }
                    else
                    {
                        var donationsService = new DonationsService(donationDTO);
                        var validationErrors = donationsService.ValidateData();
                        if (validationErrors.Count == 0)
                        {
                            var donationTableRecord = new DonationAzureTableRecord();
                            var convertionErrors = donationTableRecord.Set(donationDTO);
                            if(convertionErrors.Count == 0)
                            {
                                var insertErrors = await donationTableManager.InsertAsync(donationTableRecord);
                                if(insertErrors.Count == 0)
                                {
                                    await donationQueue.DeleteAsync(messageId);
                                }
                                else
                                {
                                    saNotification.Notify(insertErrors.ToString(), TraceLevel.Error);
                                    donationQueue.Release(messageId); // Release and will retry the messager after x time the message will go to dead letter queue
                                }
                            }
                            else
                            {
                                saNotification.Notify(convertionErrors.ToString(), TraceLevel.Error);
                                donationQueue.Release(messageId); // Release and will retry the messager after x time the message will go to dead letter queue
                            }
                        }
                        else
                        {
                            saNotification.Notify(validationErrors.ToString(), TraceLevel.Error);
                            saNotification.Notify($"Error validating JSON Donation:{donationDTO.ToJSON()}", TraceLevel.Error);
                        }
                        if (donationQueue.GetPerformanceTrackerCounter() % saNotification.NotifyEvery == 0)
                            saNotification.Notify(donationQueue.GetTrackedInformation("Donations popped from queue"));
                    }
                }
            }
            catch(System.Exception ex)
            {
                saNotification.Notify($"Donation.QueueProcessor.Console process crashed on machine {Environment.MachineName}, ex:{ex}", TraceLevel.Error);
            }
        }
    }
}
