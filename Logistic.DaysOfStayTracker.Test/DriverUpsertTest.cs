﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Logistic.DaysOfStayTracker.Core;
using Logistic.DaysOfStayTracker.Core.Common;
using Logistic.DaysOfStayTracker.Core.DayOfStays;
using Logistic.DaysOfStayTracker.Core.Drivers.Commands;
using Logistic.DaysOfStayTracker.Core.Extension;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using X.PagedList;
using Xunit;

namespace Logistic.DaysOfStayTracker.Test;

public class DriverUpsertTest
{
    [Fact]
    public async Task Update_NonChanged_VisaExpiryDate_Test()
    {
        using var createScope = await new TestServiceProviderFactory { AddDriver = true }.CreateAsync();
        
        await using var scope = createScope.Provider.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        var validators = scope.ServiceProvider.GetRequiredService<IEnumerable<IValidator<DayOfStayValidateDetail>>>();
        var updateRequest = new DriverUpsertRequest(validators)
        {
            Id = createScope.Driver!.Id,
            FirstName = "First",
            LastName = "Last"
        };

        var response = await mediator.Send(updateRequest);
        
        Assert.True(response.IsSuccess, "response.IsSuccess");
        Assert.NotEqual(createScope.Driver.FirstName, updateRequest.FirstName);
        Assert.NotEqual(createScope.Driver.LastName, updateRequest.LastName);
        Assert.NotEqual(createScope.Driver?.VisaExpiryDate?.AsDateTime(), updateRequest.VisaExpiryDate.Value);
        
        var driver = response.Value;
        Assert.Equal(updateRequest.FirstName, driver.FirstName);
        Assert.Equal(updateRequest.LastName, driver.LastName);
        Assert.Equal(createScope.Driver!.VisaExpiryDate, driver.VisaExpiryDate);
    }
    
    [Fact]
    public async Task Update_Test()
    {
        using var createScope = await new TestServiceProviderFactory { AddDriver = true }.CreateAsync();
        
        await using var scope = createScope.Provider.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        var validators = scope.ServiceProvider.GetRequiredService<IEnumerable<IValidator<DayOfStayValidateDetail>>>();
        var updateRequest = new DriverUpsertRequest(validators)
        {
            Id = createScope.Driver!.Id,
            FirstName = "First",
            LastName = "Last",
            VisaExpiryDate = UpdateProperty.Changed<DateTime?>(new DateTime(2020, 1, 1))
        };

        var response = await mediator.Send(updateRequest);
        
        Assert.True(response.IsSuccess, "response.IsSuccess");
        Assert.NotEqual(createScope.Driver.FirstName, updateRequest.FirstName);
        Assert.NotEqual(createScope.Driver.LastName, updateRequest.LastName);
        Assert.NotEqual(createScope.Driver?.VisaExpiryDate?.AsDateTime(), updateRequest.VisaExpiryDate.Value);
        
        var driver = response.Value;
        Assert.Equal(updateRequest.FirstName, driver.FirstName);
        Assert.Equal(updateRequest.LastName, driver.LastName);
        Assert.Equal(updateRequest.VisaExpiryDate.Value == null ? null : DateOnly.FromDateTime(updateRequest.VisaExpiryDate.Value.Value), driver.VisaExpiryDate);
    }
    
    [Fact]
    public async Task Create_Test()
    {
        using var createScope = await new TestServiceProviderFactory
        {
            AddDriver = false
        }.CreateAsync();
        
        await using var scope = createScope.Provider.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var driverCount = ctx.Drivers.Count();

        var validators = scope.ServiceProvider.GetRequiredService<IEnumerable<IValidator<DayOfStayValidateDetail>>>();
        var createRequest = new DriverUpsertRequest(validators)
        {
            FirstName = "First",
            LastName = "Last",
            VisaExpiryDate = UpdateProperty.Changed<DateTime?>(new DateTime(2001, 2, 2))
        };

        var r = await createRequest.AddCreateDayOfStayAsync(Guid.Empty, new DateOnly(2000, 1, 1), new DateOnly(2000, 1, 5));
        _ = r.Value;

        var response = await mediator.Send(createRequest);
        var driveCountAfterRequest = ctx.Drivers.Count();
        var dayOfStays = await ctx.DayOfStays.ToListAsync();
        
        Assert.True(response.IsSuccess, "response.IsSuccess");
        Assert.Equal(0, driverCount);
        Assert.Equal(1, driveCountAfterRequest);

        var driver = response.Value;
        Assert.Equal(createRequest.FirstName, driver.FirstName);
        Assert.Equal(createRequest.LastName, driver.LastName);
        Assert.Equal(createRequest.VisaExpiryDate.Value == null ? null : DateOnly.FromDateTime(createRequest.VisaExpiryDate.Value.Value), driver.VisaExpiryDate);
        
        Assert.Single(dayOfStays);
        Assert.Equal(new DateOnly(2000, 1, 1), dayOfStays[0].EntryDate);
        Assert.Equal(new DateOnly(2000, 1, 5), dayOfStays[0].ExitDate);
    }
}