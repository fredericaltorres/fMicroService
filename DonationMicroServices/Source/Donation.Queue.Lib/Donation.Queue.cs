using Donation.Model;
using fAzureHelper;
using System.Threading.Tasks;

namespace Donation.Queue.Lib
{
    public interface IDonationQueueEndqueue
    {
        Task EnqueueAsync(DonationDTO donationDTO);
    }

    public class DonationQueue : PerformanceTracker, IDonationQueueEndqueue
    {
        public string QueueName = "DonationQueue";
        QueueManager _queueManager;

        public DonationQueue(string storageAccountName, string storageAccessKey)
        {
            _queueManager = new QueueManager(storageAccountName, storageAccessKey, QueueName);
        }

        public async Task EnqueueAsync(DonationDTO donationDTO)
        {
            base.TrackNewItem();
            await _queueManager.EnqueueAsync(donationDTO.ToJSON());
        }

        public async Task<int> ApproximateMessageCountAsync()
        {
            return await _queueManager.ApproximateMessageCountAsync();
        }

        public async Task<(DonationDTO donationDTO, string messageId)> DequeueAsync()
        {
            var m = await _queueManager.DequeueAsync();
            if(m == null)
            {
                return (null, null);
            }
            else
            {
                base.TrackNewItem();
                var donationDTO = DonationDTO.FromJSON(m.AsString);
                return (donationDTO, m.Id);
            }
        }

        public async Task DeleteAsync(string id)
        {
            await _queueManager.DeleteAsync(id);
        }

        public void Release(string id)
        {
        }
    }
}
