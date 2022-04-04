﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Logistic.DaysOfStayTracker.Blazor.Components;
using Logistic.DaysOfStayTracker.Core.DayOfStays;
using Logistic.DaysOfStayTracker.Core.DayOfStays.Commands;
using Logistic.DaysOfStayTracker.Core.Drivers;
using Logistic.DaysOfStayTracker.Core.Drivers.Commands;
using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Logistic.DaysOfStayTracker.Blazor.Pages.Drivers;

public partial class DriverEditPage
{
    [Parameter]
    public Guid? Id { get; set; }

    [Inject]
    private IMediator Mediator { get; set; } = null!;
    
    [Inject]
    private NavigationManager Navigation { get; set; } = null!;
    
    private DriverUpsertRequest _model = null!;
    private readonly DayOfStaySearchRequest _dayOfStaySearchRequest = new()
    {
        DriverId = Guid.Empty,
        Year = DateTime.Today
    };

    private ICollection<string> _errors = Array.Empty<string>();
    
    private DayOfStayTable _dayOfStayTable = null!;

    protected override async Task OnInitializedAsync()
    {
        _model = Id == null ? new DriverUpsertRequest() : await Mediator.Send(new DriverUpsertModelGet(Id.Value));
        if (Id != null)
            _dayOfStaySearchRequest.DriverId = Id.Value;
    }

    private async Task Submit()
    {
        _errors = Array.Empty<string>();
        
        var result = await Mediator.Send(_model);
        if (result.IsFailure)
        {
            _errors = result.Error;
            return;
        }
        
        Navigation.NavigateTo("/");
    }

    private void Cancel()
    {
        Navigation.NavigateTo("/");
    }
}