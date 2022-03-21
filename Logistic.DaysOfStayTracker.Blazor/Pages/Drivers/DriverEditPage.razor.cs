﻿using System;
using System.Threading.Tasks;
using Logistic.DaysOfStayTracker.Blazor.Components;
using Logistic.DaysOfStayTracker.Core.Database;
using Logistic.DaysOfStayTracker.Core.DayOfStays;
using Logistic.DaysOfStayTracker.Core.Drivers;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace Logistic.DaysOfStayTracker.Blazor.Pages.Drivers;

public partial class DriverEditPage
{
    [Parameter]
    public Guid? Id { get; set; }

    [Inject]
    private AppDbContext AppContext { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;
    
    private Driver _model = null!;
    private readonly DayOfStaySearchRequest _dayOfStaySearchRequest = new()
    {
        DriverId = Guid.Empty,
        Year = DateTime.Today
    };
    
    private MudDatePicker _pickerStart = null!;
    private MudDatePicker _pickerEnd = null!;
    private MudDatePicker _pickerYear = null!;
    private DayOfStayTable _dayOfStayTable = null!;

    protected override async Task OnInitializedAsync()
    {
        Driver? driver = null;
        if (Id != null)
        {
            var id = Id.Value;
            _dayOfStaySearchRequest.DriverId = id;
            driver = await AppContext.Drivers.FirstOrDefaultAsync(r => r.Id == id);
        }

        driver ??= new Driver();
        _model = driver;
    }

    private async Task Submit()
    {
        // TODO validate
        if (_model.Id == Guid.Empty)
        {
            await AppContext.Drivers.AddAsync(_model);
        }
        else
        {
            var updateDriver = await AppContext.Drivers.AsTracking().FirstAsync(r => r.Id == _model.Id);
            updateDriver.FirstName = _model.FirstName;
            updateDriver.LastName = _model.LastName;
            AppContext.Drivers.Update(updateDriver);
        }

        await AppContext.SaveChangesAsync();
        Navigation.NavigateTo("/");
    }

    private Task SearchStayOfDateAsync()
    {
        return _model.Id == Guid.Empty ? Task.CompletedTask : _dayOfStayTable.SearchAsync();
    }
}