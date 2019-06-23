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
            try
            {
                await Notify(context);
            }
            catch (System.Exception ex)
            {
                await NotifyError(context, ex);
            }
            await _next(context); // Call the next delegate/middleware in the pipeline
        }

        private async Task Notify(HttpContext context)
        {
            if (context.Request.Method.ToLowerInvariant() == "post" && context.Request.Path == "/api/Donation")
            {
                __perfTracker.TrackNewItemThreadSafe();

                if (__perfTracker.ItemCountThreadSafe % SystemActivityNotificationManager.NotifyEvery == 0)
                {
                    var saNotificationPublisher = new SystemActivityNotificationManager(RuntimeHelper.GetAppSettings("connectionString:ServiceBusConnectionString"));
                    await saNotificationPublisher.NotifyPerformanceInfoAsync(SystemActivityPerformanceType.DonationEnqueued, "<!>", __perfTracker.Duration, __perfTracker.ItemPerSecond, __perfTracker.ItemCountThreadSafe);
                    await saNotificationPublisher.CloseAsync();
                }
            }
        }

        private async Task NotifyError(HttpContext context, System.Exception exception)
        {
            try
            {
                var saNotificationPublisher = new SystemActivityNotificationManager(RuntimeHelper.GetAppSettings("connectionString:ServiceBusConnectionString"));
                await saNotificationPublisher.NotifyAsync(exception.Message, SystemActivityType.Error);
                await saNotificationPublisher.CloseAsync();
            }
            catch(System.Exception ex)
            {
                var m = $"Exception:{ex.Message}, when NotifyError:{exception.Message}";
                System.Console.WriteLine(m);
                System.Diagnostics.Trace.WriteLine(m);
            }
        }

        public static async Task NotifyInfoAsync(string message, Dictionary<string, object> properties, bool sendToConsole = true)
        {
            try
            {
                var saNotificationPublisher = new SystemActivityNotificationManager(RuntimeHelper.GetAppSettings("connectionString:ServiceBusConnectionString"));                
                await saNotificationPublisher.NotifyInfoAsync(message, properties, sendToConsole);
                await saNotificationPublisher.CloseAsync();
            }
            catch (System.Exception ex)
            {
                var m = $"Exception:{ex.Message}";
                System.Console.WriteLine(m);
                System.Diagnostics.Trace.WriteLine(m);
            }
        }

        public static async Task NotifyInfoAsync(string message)
        {
            try
            {
                var saNotificationPublisher = new SystemActivityNotificationManager(RuntimeHelper.GetAppSettings("connectionString:ServiceBusConnectionString"));
                await saNotificationPublisher.NotifyInfoAsync(message);
                await saNotificationPublisher.CloseAsync();
            }
            catch (System.Exception ex)
            {
                var m = $"Exception:{ex.Message}";
                System.Console.WriteLine(m);
                System.Diagnostics.Trace.WriteLine(m);
            }
        }
    }
}
