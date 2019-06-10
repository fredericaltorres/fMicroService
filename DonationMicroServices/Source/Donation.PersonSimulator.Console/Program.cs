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

            var containerInstanceIndex = RuntimeHelper.GetCommandLineParameterInt("-containerInstanceIndex", args);
            var donationEndPointIP = RuntimeHelper.GetCommandLineParameterString("-donationEndPointIP", args);

            System.Console.WriteLine($"containerInstanceIndex:{containerInstanceIndex}, donationEndPointIP:{donationEndPointIP}");

            Publish(containerInstanceIndex, donationEndPointIP).GetAwaiter().GetResult();
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

        static string GetDonationUrl(string hostOrIp)
        {
            return $"http://{hostOrIp}:80/api/Donation";
        }

        static async Task<string> PostDonation(DonationDTO donation, string donationEndPointIP)
        {
            donation.__ProcessingMachineID = null;
            var (succeeded, location, _) = await RuntimeHelper.HttpHelper.PostJson(new Uri(GetDonationUrl(donationEndPointIP)), donation.ToJSON());
            if (succeeded)
                return location;
            else
                return null;
        }

        static async Task Publish(int generationIndex, string donationEndPointIP)
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

                // TODO: Handle better http error
                var locations = await Task.WhenAll(donation10.Select(d => PostDonation(d, donationEndPointIP)));
                if(locations.Any(location => location == null))
                {
                    await saNotification.NotifyAsync($"Error posting donation, machine/pod:{RuntimeHelper.GetMachineName()}", SystemActivityType.Error);
                }

                donations.RemoveRange(0, groupCount);
                perfTracker.TrackNewItem(groupCount);

                if (perfTracker.GetPerformanceTrackerCounter() % saNotification.NotifyEvery == 0)
                {
                    System.Console.WriteLine("");
                    await saNotification.NotifyAsync("Donation", "Posted to Entrance endpoint", perfTracker.Duration, perfTracker.ItemPerSecond, perfTracker.ItemCount);
                }
            }

            await saNotification.NotifyAsync(DS.List(
                $"{donations.Count} donations sent",
                $"End sending donation from file {donationJsonFile}"
            ));
        }
    }
}
