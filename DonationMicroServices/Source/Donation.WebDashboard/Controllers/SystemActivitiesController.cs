using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using fAzureHelper;
using Microsoft.AspNetCore.Mvc;


namespace Donation.WebDashboard.Controllers
{
    [Route("api/[controller]")]
    public class SystemActivitiesController : Controller
    {
        public static SystemActivitySummary __systemActivitySummary = new SystemActivitySummary()
        {
            LastMessage = "Nothing Recieved yet"
        };

        [HttpGet("[action]")]
        public SystemActivitySummary GetSystemActivityClearError()
        {
            __systemActivitySummary.DonationErrorsSummaryDictionary.Clear();
            return GetSystemActivitySummaryPreparedToBeSentToWebDashboard();
        }

        [HttpGet("[action]")]
        public SystemActivitySummary GetSystemActivityClearInfo()
        {
            __systemActivitySummary.DonationInfoSummaryDictionary.Clear();
            return GetSystemActivitySummaryPreparedToBeSentToWebDashboard();
        }

        /// <summary>
        /// The endpoint return the static global __systemActivitySummary
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public SystemActivitySummary GetSystemActivitySummary()
        {
            var sa = GetSystemActivitySummaryPreparedToBeSentToWebDashboard();
            WriteOutputJsonFile(sa);
            return sa;
        }

        private static void WriteOutputJsonFile(SystemActivitySummary sa)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(sa, Newtonsoft.Json.Formatting.Indented);
            var outputFile = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "SystemActivitySummary.json");
            if (System.IO.File.Exists(outputFile))
                System.IO.File.Delete(outputFile);
            System.IO.File.WriteAllText(outputFile, json);
        }

        private static SystemActivitySummary GetSystemActivitySummaryPreparedToBeSentToWebDashboard()
        {
            __systemActivitySummary.DonationSentToEndPointActivitySummaryTotals = MergeMachineNameChartDataList(__systemActivitySummary.DonationSentToEndPointActivitySummaryDictionary.Totals);
            __systemActivitySummary.DonationEnqueuedActivitySummaryTotals = MergeMachineNameChartDataList(__systemActivitySummary.DonationEnqueuedActivitySummaryDictionary.Totals);
            __systemActivitySummary.DonationProcessedActivitySummaryTotals = MergeMachineNameChartDataList(__systemActivitySummary.DonationProcessedActivitySummaryDictionary.Totals);
            return __systemActivitySummary;
        }

        private static List<ChartData> MergeMachineNameChartDataList(List<ChartData> chartDataListWithMultipleMachineName)
        {
            var r = new List<ChartData>();
            if (chartDataListWithMultipleMachineName.Count == 0)
                return r;

            var machineNames = chartDataListWithMultipleMachineName.Select(c => c.MachineName).Distinct().ToList();
            var maxValue = chartDataListWithMultipleMachineName.Max(c => c.Value);
            for(var v = 500; v <= maxValue; v += 500)
            {
                var cdTotal = new ChartData();
                foreach(var m in machineNames)
                {
                    var cd = chartDataListWithMultipleMachineName.FirstOrDefault(c => c.MachineName == m && c.Value == v);
                    if (cd == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Cannot find chart data for machineName:{m}, value:{v}");
                    }
                    else {
                        cdTotal.Value += cd.Value;
                        cdTotal.Label = cd.Label;
                    }                    
                }
                if (cdTotal.Value > 0)
                    r.Add(cdTotal);
            }

            return r;
        }

        public static void AddDonationSentToEndpoint(SystemActivity sa)
        {            
            __systemActivitySummary.DonationSentToEndPointActivitySummaryDictionary.Add(
                new DonationActivitySummary() {
                    Caption = "Pushed Donation",
                    MachineName = sa.MachineName,
                    Total = sa.PerformanceInformation.TotalItemProcessed,
                    ItemPerSecond = sa.PerformanceInformation.ItemProcessedPerSecond,
                    UTCDateTime = sa.UtcDateTime,
                }
            );
        }

        public static void AddDonationEnqueued(SystemActivity sa)
        {
            __systemActivitySummary.DonationEnqueuedActivitySummaryDictionary.Add(
                new DonationActivitySummary()
                {
                    Caption = "Donation Enqueued",
                    MachineName = sa.MachineName,
                    Total = sa.PerformanceInformation.TotalItemProcessed,
                    ItemPerSecond = sa.PerformanceInformation.ItemProcessedPerSecond,
                    UTCDateTime = sa.UtcDateTime,
                }
            );
        }

        public static void AddDonationProcessed(SystemActivity sa)
        {
            __systemActivitySummary.DonationProcessedActivitySummaryDictionary.Add(
                new DonationActivitySummary()
                {
                    Caption = "Donation Processed",
                    MachineName = sa.MachineName,
                    Total = sa.PerformanceInformation.TotalItemProcessed,
                    ItemPerSecond = sa.PerformanceInformation.ItemProcessedPerSecond,
                    UTCDateTime = sa.UtcDateTime,
                }
            );
        }

        public static void AddDonationError(SystemActivity sa)
        {
            __systemActivitySummary.DonationErrorsSummaryDictionary.Add(
                new DonationActivitySummary()
                {
                    Caption = "Errors",
                    MachineName = sa.MachineName,
                    Message = new List<string>() { sa.Message },
                    UTCDateTime = sa.UtcDateTime,
                }
            );
            // Update the DonationActivitySummary with the error message count
            var e = __systemActivitySummary.DonationErrorsSummaryDictionary[sa.MachineName.ToLowerInvariant()];
            e.Total = e.Message.Count;
        }

        public static void AddDonationInfo(SystemActivity sa)
        {
            __systemActivitySummary.DonationInfoSummaryDictionary.Add(
                new DonationActivitySummary()
                {
                    Caption = "Info",
                    MachineName = sa.MachineName,
                    Message = new List<string>() { sa.Message },
                    UTCDateTime = sa.UtcDateTime,
                }
            );
            // Update the DonationActivitySummary with the error message count
            var e = __systemActivitySummary.DonationInfoSummaryDictionary[sa.MachineName.ToLowerInvariant()];
            e.Total = e.Message.Count;
        }

        public static void AddDashboardResource(string dashboardResource, int total, string jsonData, string machineName)
        {
            __systemActivitySummary.DashboardResourceActivitySummaryDictionary.Add(
                new DonationActivitySummary()
                {
                    Caption = $"Dashboard:{dashboardResource}",
                    MachineName = machineName,
                    Total = total,
                    //JsonData = jsonData
                    JsonData = "No data for now"
                });
        }

        public class ChartData
        {
            public string Label { get; set; }
            public long Value { get; set; }
            public string MachineName { get; set; }
        }
        

        public class SystemActivitySummary
        {
            public List<ChartData> DonationSentToEndPointActivitySummaryTotals { get; set; } = new List<ChartData>();
            public DonationActivitySummaryDictionary DonationSentToEndPointActivitySummaryDictionary { get; set; } = new DonationActivitySummaryDictionary();

            public List<ChartData> DonationEnqueuedActivitySummaryTotals { get; set; } = new List<ChartData>();
            public DonationActivitySummaryDictionary DonationEnqueuedActivitySummaryDictionary { get; set; } = new DonationActivitySummaryDictionary();


            public List<ChartData> DonationProcessedActivitySummaryTotals { get; set; } = new List<ChartData>();
            public DonationActivitySummaryDictionary DonationProcessedActivitySummaryDictionary { get; set; } = new DonationActivitySummaryDictionary();


            public DonationActivitySummaryDictionary DashboardResourceActivitySummaryDictionary { get; set; } = new DonationActivitySummaryDictionary();

            
            public DonationActivitySummaryDictionary DonationErrorsSummaryDictionary { get; set; } = new DonationActivitySummaryDictionary();
            public DonationActivitySummaryDictionary DonationInfoSummaryDictionary { get; set; } = new DonationActivitySummaryDictionary();
            public string LastMessage { get; set; }
            
        }

        public class DonationActivitySummary
        {
            public DateTime UTCDateTime { get; set; }
            public int ItemPerSecond { get; set; }
            public long Total { get; set; }
            public string MachineName { get; set; }
            public string Caption { get; set; }
            public string JsonData { get; set; }
            public List<string> Message { get; set; } = new List<string>();
        }

        /// <summary>
        /// Allow to aggregate a donation activity per machine,
        /// for example we can keep track of all machines pushing donation to 
        /// the main queue, and the number/s.
        /// </summary>
        public class DonationActivitySummaryDictionary: Dictionary<string, DonationActivitySummary>
        {
            public List<ChartData> Totals = new List<ChartData>();

            public void Add(DonationActivitySummary das)
            {
                var key = das.MachineName.ToLowerInvariant();
                if (this.ContainsKey(key)) {
                    this[key].Total = das.Total;
                    this[key].ItemPerSecond = das.ItemPerSecond;
                    this[key].Caption = das.Caption;
                    this[key].JsonData = das.JsonData;
                    this[key].Message.AddRange(das.Message);
                    this[key].UTCDateTime = das.UTCDateTime;
                }
                else
                {
                    this[key] = das;
                }
                this.Totals.Add(new ChartData {
                    Value = this[key].Total, 
                    Label = $"{this[key].UTCDateTime.ToString("T")}",
                    MachineName = this[key].MachineName,
                });
            }
        }
    }
}
