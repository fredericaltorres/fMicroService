using fAzureHelper;
using System.Threading.Tasks;

namespace Donation.Queue.Lib
{
    public class DonationQueue : PerformanceTracker
    {
        public const string QueueName = "DonationQueue";
        QueueManager _queueManager;

        public DonationQueue(string storageAccountName, string storageAccessKey )
        {
            _queueManager = new QueueManager(storageAccountName, storageAccessKey, QueueName);
        }
        public async Task PushAsync(Donation.Model.DonationDTO donationDTO)
        {
            base.TrackNewItem();
            await _queueManager.EnqueueAsync(donationDTO.ToJSON());
        }
       
    }
}
