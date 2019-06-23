using System;

namespace fAzureHelper
{
    public class Error
    {
        public string Reason;
        public DateTime TimeStamp;
        public System.Exception Exception;

        public Error(string reason, System.Exception exception)
        {
            this.Reason = reason;
            this.TimeStamp = DateTime.UtcNow;
            this.Exception = exception;
        }
        
        public override string ToString()
        {
            return $"[{this.TimeStamp}] Reason:{this.Reason}";
        }
    }
}
