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
        static PerformanceTracker __perfTracker = new PerformanceTracker();

        private readonly RequestDelegate _next;

        public DonationCounterMiddleware(RequestDelegate next)
        {
            this._next = next;
            
        }

        public async Task Invoke(HttpContext context)
        {
            if(context.Request.Method.ToLowerInvariant() == "post" && context.Request.Path == "/api/Donation")
            {
                __perfTracker.TrackNewItemThreadSafe();
                
                if (__perfTracker.ItemCountThreadSafe % SystemActivityNotificationManager.NotifyEvery == 0)
                {
                    var saNotificationPublisher = new SystemActivityNotificationManager(RuntimeHelper.GetAppSettings("connectionString:ServiceBusConnectionString"));
                    await saNotificationPublisher.NotifyPerformanceInfoAsync(SystemActivityPerformanceType.DonationEnqueued, "DonationEnqueued", __perfTracker.Duration, __perfTracker.ItemPerSecond, __perfTracker.ItemCountThreadSafe);
                }
            }
            await _next(context); // Call the next delegate/middleware in the pipeline
        }
    }
}
