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
    
    [Inject]
    private AppDbContext AppContext { get; set; } = null!;

    [Parameter]
    public DayOfStaySearchRequest SearchRequest { get; set; } = new();
    
    private IList<DayOfStaySearchResponse> _items = Array.Empty<DayOfStaySearchResponse>();
    private ICollection<string>? _errors;

    protected override Task OnInitializedAsync()
    {
        return SearchAsync();
    }
    
    public async Task SearchAsync()
    {
        _errors = null;
        var result = await Mediator.Send(SearchRequest);
        result.Match(list => _items = list, errors => _errors = errors);
    }

    private async Task Delete(Guid dayOfStayId)
    {
        var entity = await AppContext.DayOfStays.FindAsync(dayOfStayId);
        if(entity == null)
            return;
        
        AppContext.DayOfStays.Remove(entity);
        await AppContext.SaveChangesAsync();
        await SearchAsync();
    }
}