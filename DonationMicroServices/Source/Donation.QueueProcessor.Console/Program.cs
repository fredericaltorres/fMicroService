using AzureServiceBusSubHelper;
using Donation.Model;
using Donation.Queue.Lib;
using Donation.Service;
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

        const string DonationSubmittedTopic = "DonationSubmittedTopic";

        static async Task ProcessDonationQueue(int generationIndex)
        {
            var systemActivityNotificatior = new SystemActivityNotificationManager(GetServiceBusConnectionString());
            systemActivityNotificatior.Notify($"Donation.QueueProcessor.Console Running");
            try
            {
                // Settings come frm the appsettings.json file
                var donationQueue = new DonationQueue(RuntimeHelper.GetAppSettings("storage:AccountName"), RuntimeHelper.GetAppSettings("storage:AccountKey"));
                while (true)
                {
                    var result = await donationQueue.DequeueAsync();
                    if (result.donationDTO == null)
                    {
                        Thread.Sleep(2 * 1000);
                    }
                    else
                    {
                        var donationsService = new DonationsService(result.donationDTO);
                        if (donationsService.ValidateData().Count == 0)
                        {
                            await donationQueue.DeleteAsync(result.messageId);
                        }
                        else
                        {
                            systemActivityNotificatior.Notify($"Error validating JSON Donation:{result.donationDTO.ToJSON()}", TraceLevel.Error);
                        }
                        if (donationQueue.GetPerformanceTrackerCounter() % 100 == 0)
                            systemActivityNotificatior.Notify(donationQueue.GetTrackedInformation("Donations popped from queue"));
                    }
                }
            }
            catch(System.Exception ex)
            {
                systemActivityNotificatior.Notify($"Donation.QueueProcessor.Console process crashed on machine {Environment.MachineName}", TraceLevel.Error);
            }
        }
    }
}
