using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public static void AddDonationSentToEndpoint(long totalDonationPushed, int donationPushedPerSecond, string machineName)
        {
            __systemActivitySummary.DonationSentToEndPointActivitySummaryDictionary.Add(
                new DonationActivitySummary() {
                    Caption = "Pushed Donation",
                    MachineName = machineName,
                    Total = totalDonationPushed,
                    ItemPerSecond = donationPushedPerSecond
                });
        }

        public static void AddDonationEnqueued(long totalDonationPushed, int donationPushedPerSecond, string machineName)
        {
            __systemActivitySummary.DonationEnqueuedActivitySummaryDictionary.Add(
                new DonationActivitySummary()
                {
                    Caption = "Donation Enqueued",
                    MachineName = machineName,
                    Total = totalDonationPushed,
                    ItemPerSecond = donationPushedPerSecond
                });
        }

        public static void AddDonationProcessed(long total, int donationPerSeconds, string machineName)
        {
            __systemActivitySummary.DonationProcessedActivitySummaryDictionary.Add(
                new DonationActivitySummary()
                {
                    Caption = "Donation Processed",
                    MachineName = machineName,
                    Total = total,
                    ItemPerSecond = donationPerSeconds,
                }
            );
        }

        public static void AddDonationError(string errorMessage, string appName, string machineName)
        {
            __systemActivitySummary.DonationErrorsSummaryDictionary.Add(
                new DonationActivitySummary()
                {
                    Caption = "Errors",
                    MachineName = machineName,
                    Message = new List<string>() { $"App:{appName}, {errorMessage}" },
                }
            );
        }

        public static void AddDonationInfo(string errorMessage, string appName, string machineName)
        {
            __systemActivitySummary.DonationInfoSummaryDictionary.Add(
                new DonationActivitySummary()
                {
                    Caption = "Errors",
                    MachineName = machineName,
                    Message = new List<string>() { $"App:{appName}, {errorMessage}" },
                }
            );
        }

        public static void AddDashboardResource(string dashboardResource, int total, string jsonData, string machineName)
        {
            __systemActivitySummary.DashboardResourceActivitySummaryDictionary.Add(
                new DonationActivitySummary()
                {
                    Caption = $"Dashboard:{dashboardResource}",
                    MachineName = machineName,
                    Total = total,
                    JsonData = jsonData
                });
        }

        [HttpGet("[action]")]
        public SystemActivitySummary GetSystemActivitySummary()
        {
            return __systemActivitySummary;
        }

        public class SystemActivitySummary
        {
            public DonationActivitySummaryDictionary DonationSentToEndPointActivitySummaryDictionary { get; set; } = new DonationActivitySummaryDictionary();
            public DonationActivitySummaryDictionary DonationEnqueuedActivitySummaryDictionary { get; set; } = new DonationActivitySummaryDictionary();
            public DonationActivitySummaryDictionary DashboardResourceActivitySummaryDictionary { get; set; } = new DonationActivitySummaryDictionary();
            public DonationActivitySummaryDictionary DonationProcessedActivitySummaryDictionary { get; set; } = new DonationActivitySummaryDictionary();
            public DonationActivitySummaryDictionary DonationErrorsSummaryDictionary { get; set; } = new DonationActivitySummaryDictionary();
            public DonationActivitySummaryDictionary DonationInfoSummaryDictionary { get; set; } = new DonationActivitySummaryDictionary();

            public string LastMessage { get; set; }
            
        }

        public class DonationActivitySummary
        {
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
            public void Add(DonationActivitySummary das)
            {
                var key = das.MachineName.ToLowerInvariant();
                if (this.ContainsKey(key)) {
                    this[key].Total = das.Total;
                    this[key].ItemPerSecond = das.ItemPerSecond;
                    this[key].Caption = das.Caption;
                    this[key].JsonData = das.JsonData;
                    this[key].Message.AddRange(das.Message);
                }
                else
                {
                    this[key] = das;
                }
            }
        }
    }
}
