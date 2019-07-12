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

            //var containerInstanceIndex = RuntimeHelper.GetCommandLineParameterInt("-containerInstanceIndex", args);
            // The console app is provisioned as Kubernetes statefulsets, therefore is pod machine name end
            // with -0, -1, -2 index, that we extract
            var containerInstanceIndex = RuntimeHelper.GetKubernetesStatefullSetMachineNamePodIndex();
            var donationEndPointIP = RuntimeHelper.GetCommandLineParameterString("-donationEndPointIP", args);
            var donationEndPointPort = RuntimeHelper.GetCommandLineParameterString("-donationEndPointPort", args);

            System.Console.WriteLine($"containerInstanceIndex:{containerInstanceIndex}, donationEndPointIP:{donationEndPointIP}, donationEndPointPort:{donationEndPointPort}");
            Publish(containerInstanceIndex, donationEndPointIP, donationEndPointPort).GetAwaiter().GetResult();
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

        static string GetDonationUrl(string hostOrIp, string port)
        {
            var protocol = "http";
            if (port == "443")
                protocol = "https";

            return $"{protocol}://{hostOrIp}:{port}/api/Donation";
        }
        static string GetFlushNotificationUrl(string hostOrIp, string port)
        {
            var protocol = "http";
            if (port == "443")
                protocol = "htttps";

            return $"{protocol}://{hostOrIp}:{port}/api/info/GetFlushNotification";
        }

        static async Task<bool> SendFlushNotificationToEndpoint(SystemActivityNotificationManager saNotification, string donationEndPointIP, string donationEndPointPort)
        {
            try
            {
                await saNotification.NotifyInfoAsync("About to send flush notification");
                var (succeeded, _) = await RuntimeHelper.HttpHelper.Get(new Uri(GetFlushNotificationUrl(donationEndPointIP, donationEndPointPort)));
                await saNotification.NotifyInfoAsync($"Sent flush notification succeeded:{succeeded}");
                if (succeeded)
                    return true;
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"PostDonation failed, ex:{ex.Message}");
            }
            return false;
        }

        static async Task<string> PostDonation(DonationDTO donation, string donationEndPointIP, string donationEndPointPort, int recursiveCallCount = 0)
        {
            try
            {                
                var jwtOptions = new JwtOptions()
                {
                    SecretKey = RuntimeHelper.GetAppSettings("jwt:secretKey"),
                    ExpiryMinutes = RuntimeHelper.GetAppSettings("jwt:expiryMinutes", 1),
                    Issuer = RuntimeHelper.GetAppSettings("jwt:issuer"),
                };

                var options = new JwtOptions();
                var configuration = RuntimeHelper.BuildAppSettingsJsonConfiguration();
                var section = configuration.GetSection("jwt");
                

                Microsoft.Extensions.Options.IOptions<JwtOptions> options2 = null;
                var jwtHandler = new JwtHandler(options2);
                var userService = new UserService(new Encrypter(), jwtHandler);
                var jsonWebToken = userService.LoginAsync(RuntimeHelper.GetMachineName(), "abcd1234!");

                donation.__EntranceMachineID = null;
                var (succeeded, location, _) = await RuntimeHelper.HttpHelper.PostJson(new Uri(GetDonationUrl(donationEndPointIP, donationEndPointPort)), donation.ToJSON());
                if (succeeded)
                    return location;
            }
            catch(System.Exception ex)
            {
                System.Console.WriteLine($"PostDonation failed, ex:{ex.Message}");
                if (recursiveCallCount < 2)
                {
                    recursiveCallCount += 1;
                    System.Console.WriteLine($"Retry PostDonation recursiveCallCount:{recursiveCallCount}");
                    return await PostDonation(donation, donationEndPointIP, donationEndPointPort, recursiveCallCount);
                }
            }
            return null;
        }

        static async Task Publish(int generationIndex, string donationEndPointIP, string donationEndPointPort)
        {
            var saNotification = new SystemActivityNotificationManager(GetServiceBusConnectionString());
            await saNotification.NotifyAsync($"starting");

            var donationJsonFile = GetGeneratedDonationDataFile(generationIndex);
            if (File.Exists(donationJsonFile))
                System.Console.WriteLine($"JSON File {donationJsonFile} ");
            else
                throw new InvalidDataException($"Cannot find file {Path.GetFileName(donationJsonFile)}");

            var donations = DonationDTOs.FromJsonFile(donationJsonFile);
            var donationTotalCount = donations.Count;
            
            await saNotification.NotifyAsync($"{RuntimeHelper.GetAppName()} start sending {donationTotalCount} Donations from file {donationJsonFile}");

            var groupCount = 10;
            var perfTracker = new PerformanceTracker();

            while (donations.Count > 0)
            {
                System.Console.Write(".");
                var donation10 = donations.Take(groupCount);
                
                var locations = await Task.WhenAll(donation10.Select(d => PostDonation(d, donationEndPointIP, donationEndPointPort)));
                if(locations.Any(location => location == null))
                {
                    await saNotification.NotifyAsync($"Error posting donation, machine/pod:{RuntimeHelper.GetMachineName()}", SystemActivityType.Error);
                }

                donations.RemoveRange(0, groupCount);
                perfTracker.TrackNewItem(groupCount);

                if (perfTracker.ItemCount % SystemActivityNotificationManager.NotifyEvery == 0)
                {
                    System.Console.WriteLine("");
                    var percentDone = 1.0 * perfTracker.ItemCount / donationTotalCount;
                    await saNotification.NotifyPerformanceInfoAsync(SystemActivityPerformanceType.DonationSentToEndPoint, $"Posted to entrance endpoint ({percentDone}% done)", perfTracker.Duration, perfTracker.ItemPerSecond, perfTracker.ItemCount);
                    await saNotification.NotifyInfoAsync(perfTracker.GetTrackedInformation("Donation sent to send endpoint"));
                }
            }

            // Wait for all simulator processes to be finished
            Thread.Sleep(60 * 1 * 1000);

            // Hopefully this 10 calls hit both processes behind the firewall
            for (var i=0; i < 10; i++)
            {                
                await SendFlushNotificationToEndpoint(saNotification, donationEndPointIP, donationEndPointPort);
            }

            await saNotification.NotifyAsync(DS.List(
                $"{donationTotalCount} donations sent",
                $"End sending donation from file {donationJsonFile}"
            ));
        }
    }
}
