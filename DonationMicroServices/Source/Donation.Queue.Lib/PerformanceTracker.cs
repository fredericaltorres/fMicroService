using System;
using System.Threading;

namespace Donation.Queue.Lib
{
    public class PerformanceTracker
    {
        private long _itemCount = 0;
        private DateTime StartTimeStamp;

        public long ItemCount
        {
            get
            {
                return this._itemCount;
            }
        }

        public long ItemCountThreadSafe
        {
            get { return Interlocked.Read(ref this._itemCount); }
        }
        
        public void TrackNewItem(int count = 1)
        {
            if (ItemCount == 0) // Set MessageSentTimeStamp on the first message that we send
                StartTimeStamp = DateTime.UtcNow;

            this._itemCount += count;
        }

        public void TrackNewItemThreadSafe(int count = 1)
        {
            // TODO: Not protected
            if (ItemCount == 0) // Set MessageSentTimeStamp on the first message that we send
                StartTimeStamp = DateTime.UtcNow;

            Interlocked.Add(ref _itemCount, count);
        }

        public void ResetTrackedInformation()
        {
            this._itemCount = 0;
        }

        public int Duration
        {
            get
            {
                return (int)(DateTime.UtcNow - StartTimeStamp).TotalSeconds;
            }
        }
        public int ItemPerSecond
        {
            get
            {
                var d = this.Duration;
                if (d == 0) return 0;
                return (int)(ItemCount / d);
            }
        }

        public string GetTrackedInformation(string action)
        {
            var duration = this.Duration;
            var messagePerSecond = this.ItemPerSecond;
            return $"{ItemCount} {action} - {duration:0.0} seconds, {messagePerSecond:0.0} message/S";
        }
        //public long GetPerformanceTrackerCounter()
        //{
        //    return ItemCount;
        //}
    }
}
