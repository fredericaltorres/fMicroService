///using Microsoft.Azure.Storage.Queue;

namespace fAzureHelper
{
    public sealed class QueueMessage
    {        
        public string Id { get; set; }
        public string PopReceipt { get; set; }
        public string AsString { get; set; }
    }
}
