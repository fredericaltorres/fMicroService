namespace fAzureHelper
{
    public enum SystemActivityPerformanceType
    {
        DonationSentToEndPoint = 1 << 0,
        DonationEnqueued = 1 << 2,
        DonationProcessed = 1 << 3,
    }
}
