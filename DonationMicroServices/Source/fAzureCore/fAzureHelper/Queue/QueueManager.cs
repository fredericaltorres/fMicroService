using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace fAzureHelper
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/azure/storage/queues/storage-dotnet-how-to-use-queues
    /// </summary>
    public class QueueManager : AzureStorageBaseClass
    {
        public string _queueName;
        CloudQueue _queue;
        private List<CloudQueueMessage> _inProcessMessages = new List<CloudQueueMessage>();

        Microsoft.WindowsAzure.Storage.Queue.CloudQueueClient _queueClient = null;

        public QueueManager(string storageAccountName, string storageAccessKey, string queueName) : base(storageAccountName, storageAccessKey)
        {
            this._queueName = queueName.ToLowerInvariant();
            this._queueClient = _storageAccount.CreateCloudQueueClient();
            this._queue = this._queueClient.GetQueueReference(this._queueName);
            this._queue.CreateIfNotExistsAsync().GetAwaiter().GetResult();
        }

        public async Task<string> EnqueueAsync(string content)
        {
            CloudQueueMessage message = new CloudQueueMessage(content);
            await this._queue.AddMessageAsync(message);

            return message.Id;
        }

        private QueueMessage BuildQueueMessage(CloudQueueMessage m)
        {
            return new QueueMessage
            {
                Id = m.Id,
                AsString = m.AsString,
                PopReceipt = m.PopReceipt
            };
        }

        public async Task<QueueMessage> PeekAsync()
        {
            CloudQueueMessage m = await this._queue.PeekMessageAsync();

            if (m == null)
                return null;

            return BuildQueueMessage(m);
        }

        public async Task<int> ApproximateMessageCountAsync()
        {
            await this._queue.FetchAttributesAsync();

            return this._queue.ApproximateMessageCount.HasValue ? this._queue.ApproximateMessageCount.Value : -1;
        }

        public async Task<List<QueueMessage>> DequeueAsync(int count)
        {
            var timeOutIsMilliSeconds = 500;
            if(count > 32)
                timeOutIsMilliSeconds = 100;
            var waitTimeForAllMessageToArraiveInQueue = new TimeSpan(0, 0, 0, 0, timeOutIsMilliSeconds);
            IEnumerable<CloudQueueMessage> messages = await this._queue.GetMessagesAsync(
                count, 
                waitTimeForAllMessageToArraiveInQueue, 
                new QueueRequestOptions {},
                new Microsoft.WindowsAzure.Storage.OperationContext {}
            );

            var l = new List<QueueMessage>();

            foreach(var m in messages)
            {
                this._inProcessMessages.Add(m);
                l.Add(BuildQueueMessage(m));
            }

            return l;
        }

        public async Task DeleteAsync(string id)
        {
            var cloudMessage = this._inProcessMessages.FirstOrDefault(m => m.Id == id);
            if (cloudMessage == null)
                throw new ApplicationException($"Cannot find queue message id:{id} in the _inProcessMessages list");

            this._inProcessMessages.Remove(cloudMessage);

            await this._queue.DeleteMessageAsync(cloudMessage);
        }

        public async Task<List<QueueMessage>> ClearAsync(int batchSize = 1)
        {
            var l = new List<QueueMessage>();
            while(await ApproximateMessageCountAsync() > 0)
            {
                var messages = await DequeueAsync(batchSize);
                l.AddRange(messages);
                foreach(var m in messages)
                    await this.DeleteAsync(m.Id);
            }

            return l;
        }
    }
}
