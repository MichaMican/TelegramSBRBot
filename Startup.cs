using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using TelegramFunFactBot.Classes;
using TelegramFunFactBot.Classes.Dapper;
using TelegramFunFactBot.Interfaces;

namespace TelegramFunFactBot
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

            services.Configure<Settings>(Configuration.GetSection("AppSettings"));

            services.AddSingleton<ICommandHandler, CommandHandler>();
            services.AddSingleton<IDapperDB, DapperDB>();
            services.AddSingleton<ITelegramAPICommunicator, TelegramAPICommunicator>();
            services.AddSingleton<IInit, Init>();
            services.AddSingleton<IHttpHandler, HttpHandler>();
            services.AddSingleton<IUpdateNotifyHandler, UpdateNotifyHandler>();

            services.AddControllersWithViews().AddNewtonsoftJson();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IUpdateNotifyHandler updateNotifyHandler)
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

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });


            if (!env.IsDevelopment())
            {
                updateNotifyHandler.checkForUpdates();
            }

        }
    }

}
