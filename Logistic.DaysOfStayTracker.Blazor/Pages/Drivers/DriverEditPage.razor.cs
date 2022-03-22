using System;
using System.Threading.Tasks;
using Logistic.DaysOfStayTracker.Blazor.Components;
using Logistic.DaysOfStayTracker.Core.DayOfStays;
using Logistic.DaysOfStayTracker.Core.Drivers;
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

    private CalculateRemainDaysResponse? _calculateRemainDaysResponse;
    
    private MudDatePicker _pickerStart = null!;
    private MudDatePicker _pickerEnd = null!;
    private MudDatePicker _pickerYear = null!;
    private DayOfStayTable _dayOfStayTable = null!;

    protected override async Task OnInitializedAsync()
    {
        _model = Id == null ? new DriverUpsertRequest() : await Mediator.Send(new DriverUpsertModelGet(Id.Value));
    }

    private async Task Submit()
    {
        await Mediator.Send(_model);
        Navigation.NavigateTo("/");
    }

    private Task SearchStayOfDateAsync()
    {
        if (_model.Id == Guid.Empty)
            return Task.CompletedTask;

        _calculateRemainDaysResponse = null;
        return  _dayOfStayTable.SearchAsync();
    }

    private async Task CalculateRemainDaysAsync()
    {
        _calculateRemainDaysResponse = await Mediator.Send(new CalculateRemainDaysRequest());
    }
}