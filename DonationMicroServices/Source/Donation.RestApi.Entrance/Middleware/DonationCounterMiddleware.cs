using Donation.Queue.Lib;
using fAzureHelper;
using fDotNetCoreContainerHelper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Donation.RestApi.Entrance.Middleware
{
    public class DonationCounterMiddleware
    {        
        static SystemActivityNotificationManager _systemActivityNotificationPublisher;
        static PerformanceTracker perfTracker;
        private readonly RequestDelegate _next;

        public DonationCounterMiddleware(RequestDelegate next)
        {
            _next = next;
            _systemActivityNotificationPublisher = new SystemActivityNotificationManager(RuntimeHelper.GetAppSettings("connectionString:ServiceBusConnectionString"));
            perfTracker = new PerformanceTracker();
        }

        public async Task Invoke(HttpContext context)
        {
            if(context.Request.Method.ToLowerInvariant() == "post" && context.Request.Path == "/api/Donation")
            {
                perfTracker.TrackNewItemThreadSafe();
                if (perfTracker.ItemCountThreadSafe % _systemActivityNotificationPublisher.NotifyEvery == 0)
                {
                    await _systemActivityNotificationPublisher.NotifyPerformanceInfoAsync(SystemActivityPerformanceType.DonationEnqueued, "DonationEnqueued", perfTracker.Duration, perfTracker.ItemPerSecond, perfTracker.ItemCountThreadSafe);
                }
            }
            await _next(context); // Call the next delegate/middleware in the pipeline
        }
    }
}
