using Donation.Queue.Lib;
using fAzureHelper;
using fDotNetCoreContainerHelper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Donation.RestApi.Entrance.Middleware
{
    public class DonationCounterMiddleware
    {
        static PerformanceTracker __perfTracker = new PerformanceTracker();
        /// <summary>
        /// https://blog.cdemi.io/async-waiting-inside-c-sharp-locks/
        /// Protect NotifyAll with a semaphone as we were losing some messages
        /// </summary>
        static SemaphoreSlim _notificationSemaphore = new SemaphoreSlim(1, 1);

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
                Console.WriteLine($"DonationCounterMiddleware ex:${ex.Message}");
                await NotifyError(context, ex);
            }
            await _next(context); // Call the next delegate/middleware in the pipeline
        }

        private async Task Notify(HttpContext context)
        {
            if (context.Request.Method.ToLowerInvariant() == "post" && context.Request.Path.ToString().ToLowerInvariant() == "/api/donation")
            {
                // NOT A GOOD IDEA FOR PERFORMANCE
                await _notificationSemaphore.WaitAsync();
                try
                {
                    var itemCount = __perfTracker.TrackNewItemThreadSafe();
                    if (itemCount % SystemActivityNotificationManager.NotifyEvery == 0)
                        await NotifyAll(itemCount, false);
                }
                finally
                {
                    _notificationSemaphore.Release();
                }
            }
            if (context.Request.Method.ToLowerInvariant() == "get" && context.Request.Path.ToString().ToLowerInvariant() == "/api/info/getflushnotification")
                await NotifyAll(__perfTracker.ItemCountThreadSafe, true);
        }

        private static async Task NotifyAll(long itemCount, bool final)
        {
            //await _notificationSemaphore.WaitAsync();
            //try
            //{
                var saNotificationPublisher = new SystemActivityNotificationManager(RuntimeHelper.GetAppSettings("connectionString:ServiceBusConnectionString"));
                await saNotificationPublisher.NotifyPerformanceInfoAsync(SystemActivityPerformanceType.DonationEnqueued, $"<!> final:{final}", 
                    __perfTracker.Duration, __perfTracker.ItemPerSecond, itemCount
                );
                await saNotificationPublisher.NotifyInfoAsync(__perfTracker.GetTrackedInformation($"Donations received by endpoint, final:{final}"));
                await saNotificationPublisher.CloseAsync();
            //}
            //finally
            //{
            //    _notificationSemaphore.Release();
            //}
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
