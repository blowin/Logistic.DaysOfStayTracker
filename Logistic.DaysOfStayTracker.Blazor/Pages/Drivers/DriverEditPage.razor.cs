using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Logistic.DaysOfStayTracker.Blazor.Components.DayOfStays;
using Logistic.DaysOfStayTracker.Core.DayOfStays.Commands;
using Logistic.DaysOfStayTracker.Core.Drivers.Commands;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace Logistic.DaysOfStayTracker.Blazor.Pages.Drivers;

public partial class DriverEditPage
{
    [Parameter]
    public Guid? Id { get; set; }

    [Inject]
    private IMediator Mediator { get; set; } = null!;
    
    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private DriverUpsertRequest Model { get; set; } = null!;
    
    private DayOfStaySearchRequest _dayOfStaySearchRequest = new(DateTime.Today.AddDays(-230), Guid.Empty);
    private CalculateRemainDaysResponse? _calculateRemainDaysResponse;
    
    private ICollection<string> _errors = Array.Empty<string>();

    private DayOfStayTable _dayOfStayTable = null!;

    protected override async Task OnInitializedAsync()
    {
        if (Id == null)
            return;
        Model = await Mediator.Send(new DriverUpsertModelGet(Id.Value));

        _dayOfStaySearchRequest = _dayOfStaySearchRequest with {DriverId = Id.Value};

        var request = new CalculateRemainDaysRequest(Id.Value, DateOnly.FromDateTime(DateTime.Today));
        _calculateRemainDaysResponse = await Mediator.Send(request);
    }

    private async Task Submit()
    {
        _errors = Array.Empty<string>();
        
        var result = await Mediator.Send(Model);
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