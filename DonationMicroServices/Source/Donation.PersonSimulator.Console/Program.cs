using AzureServiceBusSubHelper;
using Donation.Model;
using fDotNetCoreContainerHelper;
using System;
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

            var pub = new AzurePubSubManager(AzurePubSubManagerType.Publish, GetServiceBusConnectionString(), DonationSubmittedTopic);
            foreach(var donation in donations)
            {
                System.Console.WriteLine($"Pub Donation {donation.Guid} {donation.LastName} {donation.Amount}");
                await pub.PublishAsync(donation.ToJSON(), donation.Guid.ToString());

                if(pub.MessagePublishedCounter % 10 == 0)
                {
                    System.Console.WriteLine(pub.GetSendInformation());
                }                    
            }

            System.Console.WriteLine($"{donations.Count} {DonationSubmittedTopic} messages published");
        }
    }
}
