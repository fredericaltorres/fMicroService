using fDotNetCoreContainerHelper;
using System;
using System.Diagnostics;
using System.Reflection;

namespace fAzureHelper
{
    public class SystemActivity : SystemActivityEnvironment
    {
        public string AppName { get; set; }
        public string Message { get; set; }
        public DateTime UtcDateTime { get; set; }

        /// <summary>
        /// Null by default
        /// </summary>
        public SystemActivityPerformanceInformation PerformanceInformation { get; set; }
        public SystemActivityDashboardInformation DashboardInformation { get; set; }

        /// <summary>
        /// Needed for deserialization
        /// </summary>
        public SystemActivity()
        {

        }

        public SystemActivity(string message, SystemActivityType type) : base()
        {
            this.UtcDateTime = DateTime.UtcNow;
            this.AppName = RuntimeHelper.GetAppName();
            this.Type = type;
            this.Message = message;
        }
                
        public SystemActivity SetDashboardInfo(string dashboardResource, string jsonData, int totalItemProcessed)
        {
            this.Message = $"Dashboard:{dashboardResource}, totalItemProcessed:{totalItemProcessed}";
            this.Type = SystemActivityType.DashboardInfo;
            this.DashboardInformation = new SystemActivityDashboardInformation() {
                TotalItemProcessed = totalItemProcessed,
                JsonData = jsonData,
                DashboardResource = dashboardResource,
            };
            return this;
        }

        public SystemActivity SetPerformanceInfo(SystemActivityPerformanceType performanceType, string action, int durationSecond, int itemProcessedPerSeconds, long totalItemProcessed) 
        {
            this.Message = $"{totalItemProcessed} {performanceType} {action} in {durationSecond} seconds. {itemProcessedPerSeconds} / S";
            this.Type = SystemActivityType.PerformanceInfo;
            this.PerformanceInformation = new SystemActivityPerformanceInformation()
            {
                PerformanceType = performanceType,
                Action = action,
                DurationSecond = durationSecond,
                ItemProcessedPerSecond = itemProcessedPerSeconds,
                TotalItemProcessed = totalItemProcessed,
            };
            return this;
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
