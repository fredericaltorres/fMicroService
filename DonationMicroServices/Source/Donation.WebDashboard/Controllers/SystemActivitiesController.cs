using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Donation.Queue.Lib;
using fAzureHelper;
using fDotNetCoreContainerHelper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Donation.WebDashboard.Controllers
{
    [Route("api/[controller]")]
    public class SystemActivitiesController : Controller
    {
        public static SystemActivitySummary __systemActivitySummary = new SystemActivitySummary();
        
        [HttpGet("[action]")]
        public SystemActivitySummary GetSystemActivityClearError()
        {
            __systemActivitySummary.DonationErrorsSummaryDictionary.Clear();
            return GetSystemActivitySummaryPreparedToBeSentToWebDashboard();
        }

        [HttpGet("[action]")]
        public SystemActivitySummary GetSystemActivityClearAll()
        {
            __systemActivitySummary.DonationQueueCountSummaryDictionary.Clear();
            __systemActivitySummary.DonationErrorsSummaryDictionary.Clear();
            __systemActivitySummary.DonationSentToEndPointActivitySummaryDictionary.Clear();
            __systemActivitySummary.DonationEnqueuedActivitySummaryDictionary.Clear();
            __systemActivitySummary.DonationProcessedActivitySummaryDictionary.Clear();
            __systemActivitySummary.DashboardResourceActivitySummaryDictionary.Clear();
            __systemActivitySummary.DonationErrorsSummaryDictionary.Clear();
            __systemActivitySummary.DonationInfoSummaryDictionary.Clear();
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

        private static void WriteOutputJsonFile(SystemActivitySummary sa, int recursionIndex = 0)
        {
            try
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(sa, Newtonsoft.Json.Formatting.Indented);
                var outputFile = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "SystemActivitySummary.json");
                if (System.IO.File.Exists(outputFile))
                    System.IO.File.Delete(outputFile);
                System.IO.File.WriteAllText(outputFile, json);
            }
            catch(System.Exception ex)
            {
                if (recursionIndex == 0)
                {
                    Thread.Sleep(100);
                    WriteOutputJsonFile(sa, recursionIndex + 1);
                }
                else throw;
            }
        }

        private static SystemActivitySummary GetSystemActivitySummaryPreparedToBeSentToWebDashboard()
        {
            AddDonationQueueCount();
            __systemActivitySummary.DonationQueueCountSummaryDictionary.PrepareDataForDisplay();
            __systemActivitySummary.DashboardResourceActivitySummaryDictionary.PrepareDataForDisplay();
            __systemActivitySummary.DonationSentToEndPointActivitySummaryDictionary.PrepareDataForDisplay();
            __systemActivitySummary.DonationEnqueuedActivitySummaryDictionary.PrepareDataForDisplay();
            __systemActivitySummary.DonationProcessedActivitySummaryDictionary.PrepareDataForDisplay();
            __systemActivitySummary.DashboardResourceActivitySummaryDictionary.PrepareDataForDisplay();
            __systemActivitySummary.DonationErrorsSummaryDictionary.PrepareDataForDisplay();
            __systemActivitySummary.DonationInfoSummaryDictionary.PrepareDataForDisplay();

            // Aggregate in one dictionary <country, amount> all dashboard message with partial
            // country, amount  information
            var countryBreakDownHistory = new DonationCountryBreakdown();
            var dashBoardDictionary = __systemActivitySummary.DashboardResourceActivitySummaryDictionary;
            foreach (var machineCountryBreakDownInfo in dashBoardDictionary.Values)
            {
                foreach(var h in machineCountryBreakDownInfo.History)
                {
                    countryBreakDownHistory.Merge(h.JsonData);
                }
            }
            __systemActivitySummary.DonationCountryBreakdown = countryBreakDownHistory;

            return __systemActivitySummary;
        }

        private static DonationQueue GetDonationQueue()
        {
            return new DonationQueue(RuntimeHelper.GetAppSettings("storage:AccountName"), RuntimeHelper.GetAppSettings("storage:AccountKey"));
        }

        public static void AddDonationQueueCount()
        {
            DonationQueue donationQueue = GetDonationQueue();
            var queueCount = donationQueue.ApproximateMessageCountAsync().GetAwaiter().GetResult();

            __systemActivitySummary.DonationQueueCountSummaryDictionary.Add(
                new DonationActivitySummary()
                {
                    Caption = "Queue Count",
                    MachineName = "DonationQueue",
                    Total = queueCount,
                    UTCDateTime = DateTime.Now,
                }
            );
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
                    Messages = new List<string>() { sa.Message },
                    UTCDateTime = sa.UtcDateTime,
                }
            );
            // Update the DonationActivitySummary with the error message count
            var e = __systemActivitySummary.DonationErrorsSummaryDictionary[sa.MachineName.ToLowerInvariant()];
            e.Total = e.Messages.Count;
        }

        public static void AddDonationInfo(SystemActivity sa)
        {
            __systemActivitySummary.DonationInfoSummaryDictionary.Add(
                new DonationActivitySummary()
                {
                    Caption = "Info",
                    MachineName = sa.MachineName,
                    Messages = new List<string>() { sa.Message },
                    UTCDateTime = sa.UtcDateTime,
                }
            );
            // Update the DonationActivitySummary with the error message count
            var e = __systemActivitySummary.DonationInfoSummaryDictionary[sa.MachineName.ToLowerInvariant()];
            e.Total = e.Messages.Count;
        }

        public static void AddDashboardResource(string dashboardResource, int total, string jsonData, string machineName)
        {
            __systemActivitySummary.DashboardResourceActivitySummaryDictionary.Add(
                new DonationActivitySummary()
                {
                    Caption = $"Dashboard:{dashboardResource}",
                    MachineName = machineName,
                    Total = total,
                    JsonData = jsonData //+++
                });
        }

        public class SystemActivitySummary
        {
            public DonationCountryBreakdown DonationCountryBreakdown;
            public DonationActivitySummaryDictionary DonationQueueCountSummaryDictionary { get; set; } = new DonationActivitySummaryDictionary(true);

            public DonationActivitySummaryDictionary DonationSentToEndPointActivitySummaryDictionary { get; set; } = new DonationActivitySummaryDictionary(true);
            public DonationActivitySummaryDictionary DonationEnqueuedActivitySummaryDictionary { get; set; } = new DonationActivitySummaryDictionary(true);
            public DonationActivitySummaryDictionary DonationProcessedActivitySummaryDictionary { get; set; } = new DonationActivitySummaryDictionary(true);

            public DonationActivitySummaryDictionary DonationErrorsSummaryDictionary { get; set; } = new DonationActivitySummaryDictionary(false);
            public DonationActivitySummaryDictionary DonationInfoSummaryDictionary { get; set; } = new DonationActivitySummaryDictionary(false);

            [JsonIgnore]
            public DonationActivitySummaryDictionary DashboardResourceActivitySummaryDictionary { get; set; } = new DonationActivitySummaryDictionary(true);
        }

        public class DonationCountryBreakdown : Dictionary<string, double>
        {
            public void Merge(string json)
            {
                // Parse the json data
                var dic = Newtonsoft.Json.JsonConvert.DeserializeObject<DonationCountryBreakdown>(json);

                // Merge the json data with the current instance
                foreach(var e in dic)
                {
                    if(this.ContainsKey(e.Key))
                        this[e.Key] += e.Value;
                    else
                        this[e.Key] = e.Value;
                }
                if (this.ContainsKey("china"))
                    this.Remove("china");
            }
        }

        public class DonationActivityItem
        {
            public DateTime UTCDateTime { get; set; }
            public int ItemPerSecond { get; set; }
            public long Total { get; set; }
            public string MachineName { get; set; }
            public string Caption { get; set; }

            [JsonIgnore]
            public string JsonData { get; set; }
            public string ChartLabel { get; set; }
            public List<string> Messages { get; set; } = new List<string>();
        }
        public class DonationActivitySummary : DonationActivityItem
        {
            public List<DonationActivityItem> History = new List<DonationActivityItem>();

            internal DonationActivityItem Clone()
            {
                var newMessages = new List<string>();
                newMessages.AddRange(this.Messages);

                var n = new DonationActivityItem {
                    UTCDateTime   = this.UTCDateTime,
                    ItemPerSecond = this.ItemPerSecond,
                    Total         = this.Total,
                    MachineName   = this.MachineName,
                    Caption       = this.Caption,
                    JsonData      = this.JsonData,
                    ChartLabel    = this.ChartLabel,
                    Messages      = newMessages
                };
                return n;
            }
        }

        /// <summary>
        /// Allow to aggregate a donation activity per machine,
        /// for example we can keep track of all machines pushing donation to 
        /// the main queue, and the number/s.
        /// </summary>
        public class DonationActivitySummaryDictionary: Dictionary<string, DonationActivitySummary>
        {
            private readonly bool _maintainHistory;

            public DonationActivitySummaryDictionary(bool maintainHistory)
            {
                this._maintainHistory = maintainHistory;
            }

            public void PrepareDataForDisplay()
            {
                foreach(var v in this.Values)
                    v.History = v.History.OrderBy(e => e.UTCDateTime).ToList();
            }
            
            public void Add(DonationActivitySummary das)
            {                
                var key = das.MachineName.ToLowerInvariant();
                if (this.ContainsKey(key)) {

                    if (
                        // Make sure we only store the last total count, service bus message can be out of order
                        //das.Total >= this[key].Total ||

                        // Make sure we only store the newest one service bus message can be out of order
                        das.UTCDateTime > this[key].UTCDateTime
                        )
                    {
                        this[key].Total = das.Total;
                        this[key].ItemPerSecond = das.ItemPerSecond;
                        this[key].Caption = das.Caption;
                        this[key].JsonData = das.JsonData;
                        this[key].Messages.AddRange(das.Messages);
                        this[key].UTCDateTime = das.UTCDateTime;
                    }
                    else {
                        // We already recorded a newer value
                        // This instance of DonationActivitySummary came to later via Service Bus
                    }
                }
                else
                {
                    this[key] = das;
                }

                if (_maintainHistory)
                {
                    var historyDas = das.Clone();
                    historyDas.ChartLabel = $"{das.UTCDateTime.ToString("T")}";
                    this[key].History.Add(historyDas);
                }                
            }
        }
    }
}
