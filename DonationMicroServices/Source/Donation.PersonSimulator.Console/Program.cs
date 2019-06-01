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
        static void Main(string[] args)
        {
            System.Console.WriteLine(RuntimeHelper.GetContextInformation());

            if(args.Length == 0)
            {
                throw new InvalidDataException($"Command line parameter -generationIndex 0..9 required");
            }

            var generationIndex = int.Parse(args[1]);

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

            const int maxMessageToSend = 3;
            var pub = new AzurePubSubManager(AzurePubSubManagerType.Publish, GetServiceBusConnectionString(), DonationSubmittedTopic);
            foreach(var donation in donations)
            {
                await pub.PublishAsync(donation.ToJSON());
            }

            System.Console.WriteLine($"{maxMessageToSend} {DonationSubmittedTopic} messages published");
        }
    }
}
