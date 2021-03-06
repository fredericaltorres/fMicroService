﻿using AzureServiceBusSubHelper;
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
            if (port == "443" || port == "44399")
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
                System.Console.WriteLine($"SendFlushNotificationToEndpoint failed, ex:{ex.Message}");
            }

            return false;
        }

        static async Task<string> PostDonation(DonationDTO donation, string donationEndPointIP, string donationEndPointPort, string jsonWebToken,  int recursiveCallCount = 0, System.Net.Http.HttpClient sharedHttpClient = null)
        {
            try
            {                
                donation.__EntranceMachineID = null;
                var (succeeded, location, _) = await RuntimeHelper.HttpHelper.PostJson(new Uri(GetDonationUrl(donationEndPointIP, donationEndPointPort)), donation.ToJSON(), bearerToken: jsonWebToken, sharedHttpClient);
                if (succeeded)
                    return location;
            }
            catch(System.Exception ex)
            {
                System.Console.WriteLine($"PostDonation failed, ex:{ex}");
                if (recursiveCallCount < 2)
                {
                    recursiveCallCount += 1;
                    System.Console.WriteLine($"{Environment.NewLine}Retry PostDonation recursiveCallCount:{recursiveCallCount}");
                    var r = await PostDonation(donation, donationEndPointIP, donationEndPointPort, jsonWebToken, recursiveCallCount, sharedHttpClient);
                    System.Console.WriteLine($"{Environment.NewLine}Retry PostDonation recursiveCallCount:{recursiveCallCount} result:{r}");
                    return r;
                }
            }

            return null;
        }

        private static string GetJWToken()
        {
            var jwtOptions = new JwtOptions()
            {
                SecretKey = RuntimeHelper.GetAppSettings("jwt:secretKey"),
                ExpiryMinutes = RuntimeHelper.GetAppSettings("jwt:expiryMinutes", 1),
                Issuer = RuntimeHelper.GetAppSettings("jwt:issuer"),
            };
            var userService = new UserService(new Encrypter(), new JwtHandler(jwtOptions));
            var user = userService.Login(RuntimeHelper.GetMachineName(), "abcd1234!");
            var jsonWebToken = userService.GetWebToken(user).Token;

            return jsonWebToken;
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

            var donations          = DonationDTOs.FromJsonFile(donationJsonFile);
            var donationTotalCount = donations.Count;
            var groupCount         = 10;
            var perfTracker        = new PerformanceTracker();
            var sharedHttpClient   = new System.Net.Http.HttpClient();

            await saNotification.NotifyAsync($"{RuntimeHelper.GetAppName()} start sending {donationTotalCount} Donations from file {donationJsonFile}");
            await saNotification.NotifyErrorAsync($"Test Error, machine/pod:{RuntimeHelper.GetMachineName()}");

            while (donations.Count > 0)
            {
                System.Console.Write(".");
                var donation10 = donations.Take(groupCount).ToList();
                var jwtToken   = GetJWToken(); // Get one token for the next 10 donations about to be sent

                var locations  = await Task.WhenAll(donation10.Select(d => PostDonation(d, donationEndPointIP, donationEndPointPort, jwtToken, sharedHttpClient: sharedHttpClient)));
                if(locations.Any(location => location == null))
                {
                    await saNotification.NotifyErrorAsync($"Error posting donation, machine/pod:{RuntimeHelper.GetMachineName()}");
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
