using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Logistic.DaysOfStayTracker.Core;
using Logistic.DaysOfStayTracker.Core.DayOfStays.Commands;
using Logistic.DaysOfStayTracker.Core.Drivers.Commands;
using MediatR;
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
                new CalculateRemainDaysResponse(19, DateOnly.Parse("01.04.2022"), 3),
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
            yield return new object[]
            {
                new CalculateRemainDaysResponse(31, DateOnly.Parse("02.04.2022"), 7),
                DateOnly.Parse("01.04.2022"),
                new []
                {
                    (DateOnly.Parse("26.05.2021"), DateOnly.Parse("28.05.2021")),
                    (DateOnly.Parse("01.06.2021"), DateOnly.Parse("03.06.2021")),
                    (DateOnly.Parse("04.06.2021"), DateOnly.Parse("05.06.2021")),
                    (DateOnly.Parse("08.06.2021"), DateOnly.Parse("09.06.2021")),
                    (DateOnly.Parse("14.06.2021"), DateOnly.Parse("16.06.2021")),
                    (DateOnly.Parse("17.06.2021"), DateOnly.Parse("19.06.2021")),
                    (DateOnly.Parse("23.06.2021"), DateOnly.Parse("25.06.2021")),
                    (DateOnly.Parse("27.06.2021"), DateOnly.Parse("29.06.2021")),
                    (DateOnly.Parse("30.06.2021"), DateOnly.Parse("01.07.2021")),
                    (DateOnly.Parse("04.07.2021"), DateOnly.Parse("11.07.2021")),
                    (DateOnly.Parse("18.08.2021"), DateOnly.Parse("28.08.2021")),
                    (DateOnly.Parse("28.09.2021"), DateOnly.Parse("10.10.2021")),
                    (DateOnly.Parse("26.11.2021"), DateOnly.Parse("19.12.2021")),
                    (DateOnly.Parse("15.02.2022"), DateOnly.Parse("14.03.2022")),
                }
            };
            
            yield return new object[]
            {
                new CalculateRemainDaysResponse(25, DateOnly.Parse("27.03.2022"), 13),
                DateOnly.Parse("26.03.2022"),
                new []
                {
                    (DateOnly.Parse("28.09.2021"), DateOnly.Parse("10.10.2021")),
                    (DateOnly.Parse("26.11.2021"), DateOnly.Parse("19.12.2021")),
                    (DateOnly.Parse("15.02.2022"), DateOnly.Parse("14.03.2022")),
                }
            };
        }
    }

    [Theory]
    [MemberData(nameof(MemberData))]
    public async Task CalculateRemainDaysHandlerTest(CalculateRemainDaysResponse expectedResponse, DateOnly requestDate,
        (DateOnly from, DateOnly to)[] dates)
    {
        using var createScope = await new TestServiceProviderFactory().CreateAsync();
        using var scope = createScope.Provider.CreateScope();
        var sp = scope.ServiceProvider;
            
        var mediator = sp.GetRequiredService<IMediator>();
        var db = sp.GetRequiredService<AppDbContext>();
        var driver = db.Drivers.First();
            
        // init
        var upsertRequest = new DriverUpsertRequest { Id = driver.Id };
        foreach (var (from, to) in dates)
        {
            var createRequest = new DayOfStayCreateRequest(driver.Id, from, to);
            var res = await mediator.Send(createRequest);
            upsertRequest.CreateDayOfStays.Add(res.Value);
        }
        await mediator.Send(upsertRequest);
        
        // act
        var result = await mediator.Send(new CalculateRemainDaysRequest(driver.Id, requestDate));
        
        Assert.Equal(expectedResponse, result);
    }
}