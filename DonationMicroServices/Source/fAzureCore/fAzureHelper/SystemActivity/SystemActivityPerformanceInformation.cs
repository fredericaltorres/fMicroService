namespace fAzureHelper
{
    public class SystemActivityPerformanceInformation
    {
        public SystemActivityPerformanceType PerformanceType { get; set; }
        public string Action { get; set; }
        public int DurationSecond { get; set; }
        public int ItemProcessedPerSecond { get; set; }
        public long TotalItemProcessed { get; set; }
    }
}
