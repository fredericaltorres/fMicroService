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
            __systemActivitySummary.TotalDonationPushed = totalDonationPushed;
            __systemActivitySummary.DonationPushedPerSecond = donationPushedPerSecond;
            __systemActivitySummary.MachineName = machineName;
        }

        [HttpGet("[action]")]
        public SystemActivitySummary GetSystemActivitySummary()
        {
            return __systemActivitySummary;
        }

        public class SystemActivitySummary
        {
            public int DonationPushedPerSecond { get; set; }
            public int TotalDonationPushed { get; set; }
            public string MachineName { get; set; }
            public string LastMessage { get; set; }
        }
    }
}
