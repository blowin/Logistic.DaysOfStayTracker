using System.Collections.Generic;
using System.Threading.Tasks;
using Logistic.DaysOfStayTracker.Core;
using Logistic.DaysOfStayTracker.Core.DayOfStays;
using Logistic.DaysOfStayTracker.Core.DayOfStays.Commands;
using Logistic.DaysOfStayTracker.Core.Drivers.Commands;
using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Logistic.DaysOfStayTracker.Blazor.Components.DayOfStays;

public partial class DayOfStayTable
{
    [Inject]
    private IMediator Mediator { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public DayOfStaySearchRequest SearchRequest { get; set; } = null!;

    [Parameter]
    public DriverUpsertRequest DriverUpsertRequest { get; set; } = new();

    [Inject]
    public IDialogService DialogService { get; set; } = null!;

    [Inject]
    public AppDbContext Context { get; set; } = null!;
    
    private List<DayOfStay> _items = new();
    
    private MudDatePicker _pickerStart = null!;
    private MudDatePicker _pickerEnd = null!;
    
    protected override Task OnInitializedAsync()
    {
        return SearchAsync();
    }
    
    public async Task SearchAsync()
    {
        _items = await Mediator.Send(SearchRequest);
        ApplyDeleteItems();
        _items.AddRange(DriverUpsertRequest.CreateDayOfStays);
    }

    private void Delete(DayOfStay dayOfStay)
    {
        _items.Remove(dayOfStay);
        DriverUpsertRequest.AddDeletedDayOfStay(dayOfStay);
        ApplyDeleteItems();
    }

    private void ApplyDeleteItems()
    {
        _items.RemoveAll(response => DriverUpsertRequest.DeletedDayOfStays.ContainsKey(response.Id));
    }

    private void AddDayOfStay(DayOfStay createModel)
    {
        DriverUpsertRequest.CreateDayOfStays.Add(createModel);
        _items.Add(createModel);
    }
}