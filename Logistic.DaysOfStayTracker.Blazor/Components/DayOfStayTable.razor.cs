using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Logistic.DaysOfStayTracker.Core;
using Logistic.DaysOfStayTracker.Core.DayOfStays;
using MediatR;
using Microsoft.AspNetCore.Components;
using X.PagedList;

namespace Logistic.DaysOfStayTracker.Blazor.Components;

public partial class DayOfStayTable
{
    [Inject]
    private IMediator Mediator { get; set; } = null!;
    
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;
    
    [Parameter]
    public EventCallback<Guid> OnDelete { get; set; }

    [Parameter]
    public DayOfStaySearchRequest SearchRequest { get; set; } = new();

    private HashSet<Guid> _deleted = new();
    private List<DayOfStaySearchResponse> _items = new();
    private ICollection<string>? _errors;

    protected override Task OnInitializedAsync()
    {
        return SearchAsync();
    }
    
    public async Task SearchAsync()
    {
        _errors = null;
        var result = await Mediator.Send(SearchRequest);
        result.Match(
            list =>
            {
                _items = list;
                ApplyDeleteItems();
            }, 
            errors => _errors = errors);
    }

    private Task Delete(Guid dayOfStayId)
    {
        _deleted.Add(dayOfStayId);
        ApplyDeleteItems();
        return OnDelete.InvokeAsync(dayOfStayId);
    }

    private void ApplyDeleteItems()
    {
        _items.RemoveAll(response => _deleted.Contains(response.Id));
    }
}