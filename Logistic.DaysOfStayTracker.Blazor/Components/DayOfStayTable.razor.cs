using System;
using System.Linq;
using System.Threading.Tasks;
using Logistic.DaysOfStayTracker.Core.Database;
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
    
    private IPagedList<DayOfStay> _items = new StaticPagedList<DayOfStay>(Enumerable.Empty<DayOfStay>(), 1, 10, 0);
    
    protected override Task OnInitializedAsync()
    {
        return SearchAsync();
    }
    
    private Task PageChanged(int page)
    {
        SearchRequest.Page = page;
        return SearchAsync();
    }

    public async Task SearchAsync()
    {
        _items = await Mediator.Send(SearchRequest);
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