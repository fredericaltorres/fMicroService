using System;

namespace Donation.Queue.Lib
{
    public class PerformanceTracker
    {
        public int ItemCount = 0;
        private DateTime StartTimeStamp;

        public void TrackNewItem(int count = 1)
        {
            if (ItemCount == 0) // Set MessageSentTimeStamp on the first message that we send
                StartTimeStamp = DateTime.UtcNow;

            ItemCount += count;
        }

        public void ResetTrackedInformation()
        {
            ItemCount = 0;
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
                return ItemCount / d;
            }
        }

        public string GetTrackedInformation(string action)
        {
            var duration = this.Duration;
            var messagePerSecond = this.ItemPerSecond;
            return $"{ItemCount} {action} - {duration:0.0} seconds, {messagePerSecond:0.0} message/S";
        }
        public long GetPerformanceTrackerCounter()
        {
            return ItemCount;
        }
    }
}
