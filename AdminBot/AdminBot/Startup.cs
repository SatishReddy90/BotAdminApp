using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminBot.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AdminBot
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.Configure<AppSettings>(options =>
            {
                options.QnAMaker = new QnAMaker()
                {
                    Host = Configuration["AppSettings:QnAMaker:host"],
                    Service = Configuration["AppSettings:QnAMaker:service"],
                    SubscriptionKey = Configuration["AppSettings:QnAMaker:subscriptionKey"],
                    Methods = new QnAMethods()
                    {
                        Get = Configuration["AppSettings:QnAMaker:QnAMethods:get"],
                        Create = Configuration["AppSettings:QnAMaker:QnAMethods:create"],
                        Update = Configuration["AppSettings:QnAMaker:QnAMethods:update"],
                        Delete = Configuration["AppSettings:QnAMaker:QnAMethods:delete"],
                        EndPointKeys = Configuration["AppSettings:QnAMaker:QnAMethods:endpointkeys"]
                    }
                };
                options.BotApi = new BotApi()
                {
                    BaseUrl = Configuration["AppSettings:BotApi:BaseUrl"]
                };
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
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
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
