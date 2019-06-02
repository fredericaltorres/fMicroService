using AzureServiceBusSubHelper;
using Donation.Model;
using Donation.Queue.Lib;
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

            var generationIndex = RuntimeHelper.GetCommandLineParameterInt("-generationIndex", args);

            Publish(generationIndex).GetAwaiter().GetResult();
            System.Console.WriteLine("Job done waiting for ever");
            while (true)
            {
                Thread.Sleep(10 * 1000);
            }
        }

        private static string GetGeneratedDonationDataFile(int index)
        {
            return RuntimeHelper.GetAppFilePath(Path.Combine("GeneratedData", $"donation{index}.json"));
        }

        static string GetServiceBusConnectionString()
        {
            var s = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
            if (string.IsNullOrEmpty(s))
                s = RuntimeHelper.GetAppSettings("connectionString:ServiceBusConnectionString");
            return s;
        }

        const string DonationSubmittedTopic = "DonationSubmittedTopic";

        static async Task Publish(int generationIndex)
        {
            var donationJsonFile = GetGeneratedDonationDataFile(generationIndex);
            if (File.Exists(donationJsonFile))
                System.Console.WriteLine($"JSON File {donationJsonFile} ");
            else
                throw new InvalidDataException($"Cannot find file {donationJsonFile}");

            var donations = Donations.LoadFromJsonFile(donationJsonFile);

            var systemActivityNotificationManager = new SystemActivityNotificationManager(GetServiceBusConnectionString());

            await systemActivityNotificationManager.NotifyAsync($"Start sending Donation from file {donationJsonFile}");

            // Settings come frm the appsettings.json file
            var donationQueue = new DonationQueue( RuntimeHelper.GetAppSettings("storage:AccountName"), RuntimeHelper.GetAppSettings("storage:AccountKey") );

            foreach (var donation in donations)
            {
                System.Console.WriteLine($"Pub Donation {donation.Guid} {donation.LastName} {donation.Amount}");

                await donationQueue.PushAsync(donation);
                if (donationQueue.GetPerformanceTrackerCounter() % 32 == 0)
                {
                    await systemActivityNotificationManager.NotifyAsync(donationQueue.GetTrackedInformation("Donation pushed to queue"));
                }                    
            }

            await systemActivityNotificationManager.NotifyAsync(DS.List(
                $"{donations.Count} {DonationSubmittedTopic} messages published",
                $"End sending Donation from file {donationJsonFile}"
            ));
        }
    }
}
