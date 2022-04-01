using System;
using System.Linq;
using System.Threading.Tasks;
using Logistic.DaysOfStayTracker.Core;
using Logistic.DaysOfStayTracker.Core.DayOfStays;
using Logistic.DaysOfStayTracker.Core.Drivers;
using Logistic.DaysOfStayTracker.DependencyInjection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Logistic.DaysOfStayTracker.Test;

public class CalculateRemainDaysTest
{
    [Fact]
    public async Task Test()
    {
        var provider = await CreateWithDriver();
        using (var scope = provider.CreateScope())
        {
            var sp = scope.ServiceProvider;
            
            var mediator = sp.GetRequiredService<IMediator>();

            var db = sp.GetRequiredService<AppDbContext>();
            var country1Id = db.Countries.First().Id;
            var country2Id = db.Countries.Skip(1).First().Id;
            var driver = db.Drivers.First();
            
            var upsertRequest = new DriverUpsertRequest
            {
                Id = driver.Id,
                FirstName = driver.FirstName,
                LastName = driver.LastName,
            };

            var dayOfStayCreateRequests = new[]
            {
                new DayOfStayCreateRequest(driver.Id, country1Id, DateOnly.Parse("25.09.2021"), country2Id,
                    DateOnly.Parse("05.10.2021")),
                new DayOfStayCreateRequest(driver.Id, country1Id, DateOnly.Parse("22.10.2021"), country2Id,
                    DateOnly.Parse("28.10.2021")),
                new DayOfStayCreateRequest(driver.Id, country1Id, DateOnly.Parse("05.11.2021"), country2Id,
                    DateOnly.Parse("12.11.2021")),
                new DayOfStayCreateRequest(driver.Id, country1Id, DateOnly.Parse("24.12.2021"), country2Id,
                    DateOnly.Parse("19.01.2022")),
                new DayOfStayCreateRequest(driver.Id, country1Id, DateOnly.Parse("04.03.2022"), country2Id,
                    DateOnly.Parse("29.03.2022")),
            };

            foreach (var createRequest in dayOfStayCreateRequests)
            {
                var res = await mediator.Send(createRequest);
                upsertRequest.CreateDayOfStays.Add(res.Value);
            }
            
            await mediator.Send(upsertRequest);

            var result = await mediator.Send(new CalculateRemainDaysRequest( driver.Id, DateOnly.Parse("31.03.2022")));
            
            Assert.Equal(new CalculateRemainDaysResponse(11, DateOnly.Parse("31.03.2022"), 4), result);
        }
    }

    private async Task<IServiceProvider> CreateWithDriver()
    {
        var dbName = Guid.NewGuid().ToString("N");
        var collection = new ServiceCollection()
            .AddAppServices(builder =>
            {
                builder.UseInMemoryDatabase(dbName)
                    .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            })
            .BuildServiceProvider();

        using var scope = collection.CreateScope();
        
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        await scope.ServiceProvider.GetRequiredService<IMediator>().Send(new DriverUpsertRequest
        {
            FirstName = "Андрей",
            LastName = "Покров"
        });

        return collection;
    }
}