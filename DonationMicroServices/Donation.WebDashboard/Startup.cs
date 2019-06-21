using fAzureHelper;
using fDotNetCoreContainerHelper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Donation.WebDashboard
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        static SystemActivityNotificationManager systemActivityNotificationSubscriber;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
        }

        private static void SystemActivityNotificationSubscriber_OnMessageReveived(SystemActivity sa)
        {
            if (sa.Type == SystemActivityType.PerformanceInfo)
            {
                Controllers.SystemActivitiesController.AddDonationPushed(
                     sa.PerformanceInformation.TotalItemProcessed,
                     sa.PerformanceInformation.ItemProcessedPerSecond,
                     sa.MachineName
                    );
            }
            //var msg = $"[{sa.Type}] Host:{sa.MachineName}\r\n      {sa.UtcDateTime.ToShortTimeString()}, {sa.Message}";
            //Controllers.SystemActivitiesController.AddSystemActivitySummary(msg);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });

            RuntimeHelper.SetAppPath(env.ContentRootPath);

            systemActivityNotificationSubscriber = new SystemActivityNotificationManager(RuntimeHelper.GetAppSettings("connectionString:ServiceBusConnectionString"), Environment.MachineName, true);
            systemActivityNotificationSubscriber.OnMessageReceived += SystemActivityNotificationSubscriber_OnMessageReveived;
        }
    }
}
