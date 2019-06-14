using AzureServiceBusSubHelper;
using Donation.Model;
using Donation.Queue.Lib;
using Donation.Service;
using Donation.Table.Lib;
using DynamicSugar;
using fAzureHelper;
using fDotNetCoreContainerHelper;
using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

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
            ProcessDonationQueue().GetAwaiter().GetResult();
        }

        static string GetServiceBusConnectionString()
        {
            return RuntimeHelper.GetAppSettings("connectionString:ServiceBusConnectionString");
        }

        static async Task ProcessDonationQueue()
        {
            var saNotification = new SystemActivityNotificationManager(GetServiceBusConnectionString());
            saNotification.Notify($"Donation.QueueProcessor.Console Running");
            try
            {
                // Settings come frm the appsettings.json file
                var donationQueue = new DonationQueue(RuntimeHelper.GetAppSettings("storage:AccountName"), RuntimeHelper.GetAppSettings("storage:AccountKey"));
                var donationTableManager = new DonationTableManager(RuntimeHelper.GetAppSettings("storage:AccountName"), RuntimeHelper.GetAppSettings("storage:AccountKey"));    
                var donationAggregateTableManager = new DonationAggregateTableManager(RuntimeHelper.GetAppSettings("storage:AccountName"), RuntimeHelper.GetAppSettings("storage:AccountKey"));
                var donationAggregationService = new DonationsAggregationService();
                const int queuBatchSize = 10;

                while (true)
                {
                    var donations = await donationQueue.DequeueAsync(queuBatchSize);
                    if (donations.Count == 0)
                    {
                        Thread.Sleep(2 * 1000);
                    }
                    else
                    {
                        var donationsValidationService = new DonationsValidationService(donations);
                        var validationErrors = donationsValidationService.ValidateData();
                        if (validationErrors.Count == 0)
                        {
                            donationAggregationService.Add(donations);
                            donationAggregationService.AggregateData();

                            // Donation insert in azure table is not done in batch, because
                            // batch require that all record belongs to the same partition key.
                            // The partition key is the country
                            foreach (var donation in donations)
                            {
                                var donationTableRecord = new DonationAzureTableRecord();
                                var convertionErrors = donationTableRecord.Set(donation);
                                if (convertionErrors.NoError)
                                {
                                    donationTableRecord.ProcessState = DonationDataProcessState.ApprovedForSubmission;
                                    var insertErrors = await donationTableManager.InsertAsync(donationTableRecord);
                                    if (insertErrors.NoError)
                                    {
                                        await donationQueue.DeleteAsync(donation);
                                    }
                                    else
                                    {
                                        saNotification.Notify(insertErrors.ToString(), SystemActivityType.Error);
                                        donationQueue.Release(donation);
                                    }
                                }
                                else
                                {
                                    saNotification.Notify(convertionErrors.ToString(), SystemActivityType.Error);
                                    donationQueue.Release(donation); // Release and will retry the messager after x time the message will go to dead letter queue
                                }
                            }
                        }
                        else
                        {
                            saNotification.Notify($"Error validating {donations.Count} donations, errors:{validationErrors.ToString()}", SystemActivityType.Error);
                            donationQueue.Release(donations); // Release and will retry the messager after x time the message will go to dead letter queue                            
                        }
                        if (donationQueue.GetPerformanceTrackerCounter() % saNotification.NotifyEvery == 0)
                        {
                            await saNotification.NotifyAsync("donationQueue", "processed from queue", donationQueue.Duration, donationQueue.ItemPerSecond, donationQueue.ItemCount);

                            await donationAggregateTableManager.InsertAsync(new DonationAggregateAzureTableRecord(donationAggregationService.CountryAggregateData, donations.Count));
                            await saNotification.NotifyAsync($"AggregateComputation:{donationAggregationService.CountryAggregateData.ToJSON()}", SystemActivityType.DashboardInfo);
                            donationAggregationService.Clear();

                            saNotification.Notify(donationQueue.GetTrackedInformation("Donations processed from queue"));
                        }
                    }
                }
            }
            catch(System.Exception ex)
            {
                saNotification.Notify($"Donation.QueueProcessor.Console process crashed on machine {Environment.MachineName}, ex:{ex}", SystemActivityType.Error );
            }
        }
    }
}
