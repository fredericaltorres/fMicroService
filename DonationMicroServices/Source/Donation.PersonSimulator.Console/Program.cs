using AzureServiceBusSubHelper;
using Donation.Model;
using Donation.Model.Lib.Util;
using Donation.Queue.Lib;
using Donation.Service;
using DynamicSugar;
using fAzureHelper;
using fDotNetCoreContainerHelper;
using System;
using System.Diagnostics;
using System.Linq;
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
            return RuntimeHelper.GetAppSettings("connectionString:ServiceBusConnectionString");
        }

        const string DONATION_POST_URL = "http://52.167.58.190:80/api/Donation";
        static async Task<string> PostDonation(DonationDTO donation)
        {
            var (succeeded, location, _) = await RuntimeHelper.HttpHelper.PostJson(new Uri(DONATION_POST_URL), donation.ToJSON());
            if (succeeded)
                return location;
            else
                return null;
        }

        static async Task Publish(int generationIndex)
        {
            var donationJsonFile = GetGeneratedDonationDataFile(generationIndex);
            if (File.Exists(donationJsonFile))
                System.Console.WriteLine($"JSON File {donationJsonFile} ");
            else
                throw new InvalidDataException($"Cannot find file {donationJsonFile}");

            var donations = DonationDTOs.FromJsonFile(donationJsonFile);
            var saNotification = new SystemActivityNotificationManager(GetServiceBusConnectionString());

            saNotification.Notify($"Start sending Donation from file {donationJsonFile}");

            var groupCount = 10;
            var perfTracker = new PerformanceTracker();

            while (donations.Count > 0)
            {
                System.Console.Write(".");
                var donation10 = donations.Take(groupCount);
                var locations = await Task.WhenAll(donation10.Select(d => PostDonation(d)));
                donations.RemoveRange(0, groupCount);
                perfTracker.TrackNewItem(groupCount);

                if (perfTracker.GetPerformanceTrackerCounter() % saNotification.NotifyEvery == 0)
                {
                    System.Console.WriteLine("");
                    await saNotification.NotifyAsync("Donation", "Post to Entrace endpoint", perfTracker.Duration, perfTracker.ItemPerSecond, perfTracker.ItemCount);
                }
            }

            await saNotification.NotifyAsync(DS.List(
                $"{donations.Count} messages published",
                $"End sending Donation from file {donationJsonFile}"
            ));
        }
    }
}
