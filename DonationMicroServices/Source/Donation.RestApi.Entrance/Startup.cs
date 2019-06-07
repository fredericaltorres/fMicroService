using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Donation.Queue.Lib;
using fDotNetCoreContainerHelper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Donation.RestApi.Entrance
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // AddTransient, AddScoped and AddSingleton Services Differences?
            // https://stackoverflow.com/questions/38138100/addtransient-addscoped-and-addsingleton-services-differences
            // services.AddScoped new for each request
            // services.AddSingleton same for all object and request
            // services.AddTransient new every call
            services.AddScoped<IDonationQueueEndqueue, DonationQueue>();
            services.AddTransient<IDonationQueueEndqueue, DonationQueue>((ctx) =>
            {
                var donationQueue = new DonationQueue(RuntimeHelper.GetAppSettings("storage:AccountName"), RuntimeHelper.GetAppSettings("storage:AccountKey"));
                return donationQueue;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            RuntimeHelper.SetAppPath(env.ContentRootPath);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
