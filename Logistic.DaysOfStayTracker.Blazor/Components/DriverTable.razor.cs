using System;
using System.Linq;
using System.Threading.Tasks;
using Logistic.DaysOfStayTracker.Core;
using Logistic.DaysOfStayTracker.Core.Database;
using Logistic.DaysOfStayTracker.Core.Drivers;
using MediatR;
using Microsoft.AspNetCore.Components;
using X.PagedList;

namespace Logistic.DaysOfStayTracker.Blazor.Components;

public partial class DriverTable
{
    [Inject]
    private IMediator Mediator { get; set; } = null!;
    
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;
    
    [Inject]
    private AppDbContext AppContext { get; set; } = null!;

    [Parameter]
    public DriverSearchRequest SearchRequest { get; set; } = new();
    
    private IPagedList<Driver> _items = Constants.CreateEmptyPagedList<Driver>();
    
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

    private void Edit(Guid id) => NavigationManager.NavigateTo("driver/" + id);

    private async Task Delete(Guid driverId)
    {
        // TODO вынести в Mediatr + удалять все связанные сущности
        var entity = await AppContext.Drivers.FindAsync(driverId);
        if(entity == null)
            return;
        
        AppContext.Drivers.Remove(entity);
        await AppContext.SaveChangesAsync();
        await SearchAsync();
    }
}