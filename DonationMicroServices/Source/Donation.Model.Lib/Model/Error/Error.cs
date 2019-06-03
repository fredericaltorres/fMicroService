using System;

namespace Donation.Model
{
    public class Error
    {
        public string Reason;
        public DateTime TimeStamp;

        public Error(string reason)
        {
            this.Reason = reason;
            this.TimeStamp = DateTime.UtcNow;
        }
        
        public override string ToString()
        {
            return $"[{this.TimeStamp}] Reason:{this.Reason}";
        }
    }
}
