using System;
using System.Diagnostics;
using System.Reflection;

namespace fAzureHelper
{
    public class SystemActivityPerformanceInformation
    {
        public string Resource { get; set; }
        public string Action { get; set; }
        public int DurationSecond { get; set; }
        public int ItemProcessedPerSecond { get; set; }
        public int TotalItemProcessed { get; set; }
    }
    public class SystemActivity : SystemActivityEnvironment
    {
        public string AppName { get; set; }
        public string Message { get; set; }
        public DateTime UtcDateTime { get; set; }

        /// <summary>
        /// Null by default
        /// </summary>
        public SystemActivityPerformanceInformation PerformanceInformation { get; set; }

        /// <summary>
        /// Needed for deserialization
        /// </summary>
        public SystemActivity()
        {

        }

        public SystemActivity(string message, SystemActivityType type) : base()
        {
            this.UtcDateTime = DateTime.UtcNow;
            this.AppName = Assembly.GetEntryAssembly().FullName;
            this.Type = type;
            this.Message = message;
        }

        public SystemActivity(string resource, string action, int durationSecond, int itemProcessedPerSeconds, int totalItemProcessed) : this("", SystemActivityType.PerformanceInfo)
        {
            this.Message = $"{totalItemProcessed} {resource}s {action} in {durationSecond} seconds. {itemProcessedPerSeconds} / S";
            this.PerformanceInformation = new SystemActivityPerformanceInformation()
            {
                Resource = resource,
                Action = action,
                DurationSecond = durationSecond,
                ItemProcessedPerSecond = itemProcessedPerSeconds,
                TotalItemProcessed = totalItemProcessed
            };
        }

        public string ToJSON()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }
        public static SystemActivity FromJson(string json)
        {
            try
            {
                var sa = Newtonsoft.Json.JsonConvert.DeserializeObject<SystemActivity>(json);
                return sa;
            }
            catch(System.Exception ex)
            {
                Console.WriteLine($"[error]{ex}"); // TODO: We should do better
                return null;
            }
        }
    }
}
