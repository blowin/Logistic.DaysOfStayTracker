using System;
using System.Threading.Tasks;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Logistic.DaysOfStayTracker.Core;
using Logistic.DaysOfStayTracker.DependencyInjection;
using Logistic.DaysOfStayTracker.Migration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MudBlazor.Services;

namespace Logistic.DaysOfStayTracker.Blazor
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddMudServices();

            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            var appServicesConfig = new AppServicesConfiguration(builder =>
            {
                builder.UseSqlite(connectionString, b =>
                {
                    b.MigrationsAssembly(typeof(AppDesignTimeDbContextFactory).Assembly.FullName);
                });
            });
            services.AddAppServices(appServicesConfig);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.MigrateAsync();
            }
            
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

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
            
            if (HybridSupport.IsElectronActive)
            {
                Task.Run(async () =>
                {
                    await Electron.WindowManager.CreateBrowserViewAsync();
                    await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
                    {
                        MinWidth = 700,
                        MinHeight = 500,
                        Center = true
                    });
                });   
            }
        }
    }
}