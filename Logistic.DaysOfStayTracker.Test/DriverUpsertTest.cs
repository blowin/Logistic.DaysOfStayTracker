using System;
using System.Linq;
using System.Threading.Tasks;
using Logistic.DaysOfStayTracker.Core;
using Logistic.DaysOfStayTracker.Core.Drivers.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Logistic.DaysOfStayTracker.Test;

public class DriverUpsertTest
{
    [Fact]
    public async Task CreateTest()
    {
        using var createScope = await new TestServiceProviderFactory
        {
            AddDriver = false
        }.CreateAsync();
        
        await using var scope = createScope.Provider.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var driverCount = ctx.Drivers.Count();

        var createRequest = new DriverUpsertRequest
        {
            FirstName = "First",
            LastName = "Last",
            VisaExpiryDate = new DateTime(2001, 2, 2)
        };

        var response = await mediator.Send(createRequest);
        var driveCountAfterRequest = ctx.Drivers.Count();
        
        Assert.True(response.IsSuccess, "response.IsSuccess");
        Assert.Equal(0, driverCount);
        Assert.Equal(1, driveCountAfterRequest);

        var driver = response.Value;
        Assert.Equal(createRequest.FirstName, driver.FirstName);
        Assert.Equal(createRequest.LastName, driver.LastName);
        Assert.Equal(createRequest.VisaExpiryDate == null ? null : DateOnly.FromDateTime(createRequest.VisaExpiryDate.Value), driver.VisaExpiryDate);
    }
}