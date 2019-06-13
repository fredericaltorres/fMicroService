using Donation.Model;
using fAzureHelper;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<DonationDTOs> DequeueAsync(int count)
        {
            var messages = await _queueManager.DequeueAsync(count);
            if(messages.Count == 0)
            {
                return new DonationDTOs();
            }
            else
            {
                base.TrackNewItem(count);
                return new DonationDTOs( messages.Select(
                    m => {
                        var d = DonationDTO.FromJSON(m.AsString);
                        d.__QueueMessageID = m.Id;
                        return d;
                    }
                ));
            }
        }

        public async Task DeleteAsync(string id)
        {
            await _queueManager.DeleteAsync(id);
        }

        public void Release(string id)
        {
        }
        public void Release(IEnumerable<string> id)
        {
        }
    }
}
