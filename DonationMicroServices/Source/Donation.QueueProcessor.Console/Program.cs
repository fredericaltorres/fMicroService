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
            await saNotification.NotifyInfoAsync($"starting");            
            try
            {
                var donationQueue                          = new DonationQueue(RuntimeHelper.GetAppSettings("storage:AccountName"), RuntimeHelper.GetAppSettings("storage:AccountKey"));
                var donationTableManager                   = new DonationTableManager(RuntimeHelper.GetAppSettings("storage:AccountName"), RuntimeHelper.GetAppSettings("storage:AccountKey"));    
                var donationAggregateTableManager          = new DonationAggregateTableManager(RuntimeHelper.GetAppSettings("storage:AccountName"), RuntimeHelper.GetAppSettings("storage:AccountKey"));
                var donationAggregationService             = new DonationsAggregationService();
                var queuBatchSize                          = 10;
                var sleepDurationInSecondWhenNoItemInQueue = 4;
                var lastTimeDonationWereProcessed          = DateTime.Now;
                var sendFinalNotification                  = true;   // If true we need to send the final notification to web dashboard
                var monitorIdleProcess                     = false;  // Should we start monitoring for idle mode after having processed donation
                var maxIdleMinutesToSendFinalNotification  = 1;

                while (true)
                {
                    var donations = await donationQueue.DequeueAsync(queuBatchSize);
                    if (donations.Count == 0)
                    {
                        // No donation in the queue, let's wait and sleep
                        Thread.Sleep(sleepDurationInSecondWhenNoItemInQueue * 1000);

                        // Check if we need to send the final notification to the web dashboard
                        if(monitorIdleProcess && sendFinalNotification && ((DateTime.Now - lastTimeDonationWereProcessed).Minutes > maxIdleMinutesToSendFinalNotification))
                        {
                            // Send the final notification after 3 minutes being idle
                            await NotifyBatchProcessedAsync(saNotification, donationQueue, donationAggregateTableManager, donationAggregationService, true);
                            sendFinalNotification = false; // we sent the final notification, we do not need to do it again
                        }
                    }
                    else
                    {
                        monitorIdleProcess             = true; // We processed at least our first donation, so now we can start waiting for the idle mode
                        lastTimeDonationWereProcessed  = DateTime.Now; // Mark the last time we popped and processed notification
                        var donationsValidationService = new DonationsValidationService(donations);
                        var validationErrors           = donationsValidationService.ValidateData();

                        if (validationErrors.Count == 0)
                        {
                            donationAggregationService.Add(donations);
                            donationAggregationService.AggregateData();

                            // Donation insert in azure table is not done in batch, because
                            // batch require that all record belongs to the same partition key.
                            // The partition key is the country
                            foreach (var donation in donations)
                            {
                                donation.__ProcessingMachineID = RuntimeHelper.GetMachineName();
                                var donationTableRecord = new DonationAzureTableRecord();
                                var convertionErrors = donationTableRecord.Set(donation);
                                if (convertionErrors.NoError)
                                {
                                    donationTableRecord.ProcessState = DonationDataProcessState.ApprovedForSubmission;
                                    var insertErrors = await donationTableManager.InsertAsync(donationTableRecord);
                                    if (insertErrors.NoError)
                                    {
                                        await donationQueue.DeleteAsync(donation); // Delete the donation from the queue after processing
                                    }
                                    else
                                    {
                                        await saNotification.NotifyErrorAsync(insertErrors);
                                        donationQueue.Release(donation);  // Release and will retry the messager after x time the message will go to dead letter queue
                                    }
                                }
                                else
                                {
                                    await saNotification.NotifyErrorAsync(convertionErrors);
                                    donationQueue.Release(donation); // Release and will retry the messager after x time the message will go to dead letter queue
                                }
                            }
                        }
                        else
                        {
                            await saNotification.NotifyErrorAsync($"Error validating {donations.Count} donations, errors:{validationErrors.ToString()}");
                            donationQueue.Release(donations); // Release and will retry the messager after x time the message will go to dead letter queue                            
                        }

                        if (donationQueue.ItemCount % SystemActivityNotificationManager.NotifyEvery == 0)
                        {
                            await NotifyBatchProcessedAsync(saNotification, donationQueue, donationAggregateTableManager, donationAggregationService, false);
                        }
                    }
                }
            }
            catch(System.Exception ex)
            {
                await saNotification.NotifyErrorAsync($"Process crashed on machine {RuntimeHelper.GetMachineName()}, ex:{ex.Message}", ex);
            }
        }

        private static async Task NotifyBatchProcessedAsync(SystemActivityNotificationManager saNotification, DonationQueue donationQueue, DonationAggregateTableManager donationAggregateTableManager, DonationsAggregationService donationAggregationService, bool final)
        {
            await donationAggregateTableManager.InsertAsync(new DonationAggregateAzureTableRecord(donationAggregationService.CountryAggregateData, donationAggregationService.AggregatedRecordCount));
            await saNotification.NotifyPerformanceInfoAsync(SystemActivityPerformanceType.DonationProcessed, $"processed from queue (final:{final})", donationQueue.Duration, donationQueue.ItemPerSecond, donationQueue.ItemCount);
            await saNotification.NotifySetDashboardInfoInfoAsync("CountryAggregate", donationAggregationService.CountryAggregateData.ToJSON(), donationAggregationService.AggregatedRecordCount);
            await saNotification.NotifyInfoAsync(donationQueue.GetTrackedInformation($"Donations processed from queue (final:{final})"));
            donationAggregationService.Clear();
        }
    }
}
