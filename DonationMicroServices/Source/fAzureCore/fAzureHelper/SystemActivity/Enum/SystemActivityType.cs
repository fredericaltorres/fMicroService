namespace fAzureHelper
{
    public enum SystemActivityType
    {
        CrititalError = 1 << 0,
        Error = 1 << 2,
        Warning = 1 << 3,
        Info = 1 << 4,
        PerformanceInfo = 1 << 5,
        DashboardInfo = 1 << 6,
    }
}
