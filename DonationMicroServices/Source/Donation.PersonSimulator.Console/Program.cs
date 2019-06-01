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

            var pub = new AzurePubSubManager(AzurePubSubManagerType.Publish, GetServiceBusConnectionString(), DonationSubmittedTopic);
            var counter = 0;
            var startTime = DateTime.UtcNow;
            foreach(var donation in donations)
            {
                System.Console.WriteLine($"Pub Donation {donation.Guid} {donation.LastName} {donation.Amount}");
                await pub.PublishAsync(donation.ToJSON(), donation.Guid.ToString());
                counter++;
                if(counter % 10 == 0)
                {
                    var duration = (DateTime.UtcNow - startTime).TotalSeconds;
                    var messagePerSecond = counter / duration;
                    System.Console.WriteLine($"{counter} / {donations.Count} messages published {duration:0.0} seconds, {messagePerSecond:0.0} message/S");
                }                    
            }

            System.Console.WriteLine($"{donations.Count} {DonationSubmittedTopic} messages published");
        }
    }
}
