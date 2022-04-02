using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Logistic.DaysOfStayTracker.Core;
using Logistic.DaysOfStayTracker.Core.Countries;
using Logistic.DaysOfStayTracker.Core.DayOfStays;
using Logistic.DaysOfStayTracker.Core.DayOfStays.Commands;
using Logistic.DaysOfStayTracker.Core.Drivers;
using Logistic.DaysOfStayTracker.Core.Drivers.Commands;
using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using X.PagedList;

namespace Logistic.DaysOfStayTracker.Blazor.Components;

public partial class DayOfStayTable
{
    [Inject]
    private IMediator Mediator { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public DayOfStaySearchRequest SearchRequest { get; set; } = new();

    [Parameter]
    public DriverUpsertRequest DriverUpsertRequest { get; set; } = new();

    [Inject]
    public IDialogService DialogService { get; set; } = null!;

    [Inject]
    public AppDbContext Context { get; set; } = null!;
    
    private List<DayOfStay> _items = new();
    private ICollection<string>? _errors;
    private Dictionary<Guid, Country> _countries = null!;
    private CalculateRemainDaysResponse? _calculateRemainDaysResponse;
    
    private MudDatePicker _pickerStart = null!;
    private MudDatePicker _pickerEnd = null!;
    private MudDatePicker _pickerYear = null!;
    
    protected override async Task OnInitializedAsync()
    {
        var countries = await Context.Countries.ToListAsync();
        _countries = countries.ToDictionary(e => e.Id);
        
        var request = DriverUpsertRequest.Id != null ?
            new CalculateRemainDaysRequest(DriverUpsertRequest.Id.Value, DateOnly.FromDateTime(DateTime.Today)) :
            null;
        
        if(request != null)
            _calculateRemainDaysResponse = await Mediator.Send(request);
        
        await SearchAsync();
    }
    
    public async Task SearchAsync()
    {
        _errors = null;
        var result = await Mediator.Send(SearchRequest);
        
        result.Match(list =>
            {
                _items = list;
                ApplyDeleteItems();
                _items.AddRange(DriverUpsertRequest.CreateDayOfStays);
            }, 
            errors => _errors = errors);
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

    private async Task AddDayOfStay()
    {
        var op = new DialogOptions{ FullWidth = true };

        if (_countries.Count == 0)
        {
            var countries = await Context.Countries.ToListAsync();
            _countries = countries.ToDictionary(e => e.Id);
        }
        var parameters = CreateDayOfStayDialog.CreateParameters(_countries.Select(e => e.Value).ToList(), 
            SearchRequest.DriverId ?? Guid.Empty, Mediator);
        
        var dialog = DialogService.Show<CreateDayOfStayDialog>("Создать", parameters, op);
        var result = await dialog.Result;
        if(result.Cancelled)
            return;

        var createModel = (DayOfStay) result.Data;
        DriverUpsertRequest.CreateDayOfStays.Add(createModel);
        _items.Add(createModel);
    }
}