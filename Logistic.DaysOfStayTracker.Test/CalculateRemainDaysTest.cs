using System;
using System.Collections.Generic;
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
    public static IEnumerable<object[]> MemberData
    {
        get
        {
            yield return new object[]
            {
                new CalculateRemainDaysResponse(11, DateOnly.Parse("31.03.2022"), 4),
                DateOnly.Parse("31.03.2022"),
                new []
                {
                    (DateOnly.Parse("25.09.2021"), DateOnly.Parse("05.10.2021")),
                    (DateOnly.Parse("22.10.2021"), DateOnly.Parse("28.10.2021")),
                    (DateOnly.Parse("05.11.2021"), DateOnly.Parse("12.11.2021")),
                    (DateOnly.Parse("24.12.2021"), DateOnly.Parse("19.01.2022")),
                    (DateOnly.Parse("04.03.2022"), DateOnly.Parse("29.03.2022")),
                }
            };
        }
    }

    [Theory]
    [MemberData(nameof(MemberData))]
    public async Task CalculateRemainDaysHandlerTest(CalculateRemainDaysResponse expectedResponse, DateOnly requestDate,
        (DateOnly from, DateOnly to)[] dates)
    {
        var provider = await CreateWithDriver();
        using var scope = provider.CreateScope();
        var sp = scope.ServiceProvider;
            
        var mediator = sp.GetRequiredService<IMediator>();

        var db = sp.GetRequiredService<AppDbContext>();
        var countryId = db.Countries.First().Id;
        var driver = db.Drivers.First();
            
        var upsertRequest = new DriverUpsertRequest { Id = driver.Id };

        foreach (var (from, to) in dates)
        {
            var createRequest = new DayOfStayCreateRequest(driver.Id, countryId, from, countryId, to);
            var res = await mediator.Send(createRequest);
            upsertRequest.CreateDayOfStays.Add(res.Value);
        }
            
        await mediator.Send(upsertRequest);

        var result = await mediator.Send(new CalculateRemainDaysRequest(driver.Id, requestDate));
        Assert.Equal(expectedResponse, result);
    }

    private static async Task<IServiceProvider> CreateWithDriver()
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