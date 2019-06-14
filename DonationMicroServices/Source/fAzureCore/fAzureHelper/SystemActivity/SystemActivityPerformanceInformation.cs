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
}
