using System;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Logistic.DaysOfStayTracker.Core.Database;
using Logistic.DaysOfStayTracker.Core.DayOfStays;
using Logistic.DaysOfStayTracker.Core.Drivers;
using Logistic.DaysOfStayTracker.DependencyInjection;
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

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddMudServices();

            services.AddAppServices(builder => builder.UseInMemoryDatabase("db"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.EnsureCreatedAsync();
                await Initialize(db);
                await db.SaveChangesAsync();
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
        }

        private async Task Initialize(AppDbContext db)
        {
            var driverFaker = new Faker<Driver>("ru").UseSeed(8080)
                .RuleFor(e => e.Id, e => e.Random.Guid())
                .RuleFor(e => e.FirstName, e => e.Person.FirstName)
                .RuleFor(e => e.LastName, e => e.Person.LastName);

            const int driverCount = 100;
            
            var drivers = driverFaker.GenerateForever().Take(driverCount).ToList();
            await db.Drivers.AddRangeAsync(drivers);

            var dayOfStaysFaker = new Faker<DayOfStay>("ru").UseSeed(8080)
                .RuleFor(e => e.Id, e => e.Random.Guid())
                .RuleFor(e => e.DriverId, e => e.PickRandom(drivers).Id)
                .Rules((faker, stay) =>
                {
                    var year = faker.Date.PastOffset();
                    stay.Start = DateOnly.FromDateTime(year.Date);
                    stay.End = faker.Date.BetweenDateOnly(stay.Start, stay.Start.AddYears(1));
                });

            var dayOfStays = dayOfStaysFaker.GenerateForever().Take(5 * driverCount).ToList();
            await db.DayOfStays.AddRangeAsync(dayOfStays);
        }
    }
}