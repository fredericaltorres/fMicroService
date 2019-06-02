using System;

namespace Donation.Queue.Lib
{
    public class PerformanceTracker
    {
        private long _trackingCounter = 0;
        private DateTime StartTimeStamp;

        public void TrackNewItem()
        {
            if (_trackingCounter == 0) // Set MessageSentTimeStamp on the first message that we send
                StartTimeStamp = DateTime.UtcNow;

            _trackingCounter++;
        }
        public string GetTrackedInformation(string action)
        {
            var duration = (DateTime.UtcNow - StartTimeStamp).TotalSeconds;
            var messagePerSecond = _trackingCounter / duration;
            return $"{_trackingCounter} {action} - {duration:0.0} seconds, {messagePerSecond:0.0} message/S";
        }
        public long GetPerformanceTrackerCounter()
        {
            return _trackingCounter;
        }
    }
}
