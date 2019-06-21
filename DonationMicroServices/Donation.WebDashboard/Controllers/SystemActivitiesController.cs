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

        public static void AddDonationPushed(int totalDonationPushed, int donationPushedPerSecond, string machineName)
        {
            __systemActivitySummary.PushedDonationActivitySummaryDictionary.Add(
                new DonationActivitySummary() {
                    Caption = "Pushed Donation",
                    MachineName = machineName,
                    Total = totalDonationPushed,
                    ItemPerSecond = donationPushedPerSecond
                });
        }

        [HttpGet("[action]")]
        public SystemActivitySummary GetSystemActivitySummary()
        {
            return __systemActivitySummary;
        }

        public class SystemActivitySummary
        {
            public DonationActivitySummaryDictionary PushedDonationActivitySummaryDictionary { get; set; } = new DonationActivitySummaryDictionary();
            public string LastMessage { get; set; }
        }

        public class DonationActivitySummary
        {
            public int ItemPerSecond { get; set; }
            public int Total { get; set; }
            public string MachineName { get; set; }
            public string Caption { get; set; }
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
                }
                else
                {
                    this[key] = das;
                }
            }
        }
    }
}
