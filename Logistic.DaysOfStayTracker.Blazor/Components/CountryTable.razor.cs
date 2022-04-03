using System;
using System.Threading.Tasks;
using Logistic.DaysOfStayTracker.Blazor.Extension;
using Logistic.DaysOfStayTracker.Core;
using Logistic.DaysOfStayTracker.Core.Countries;
using Logistic.DaysOfStayTracker.Core.Countries.Commands;
using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using X.PagedList;
using Constants = Logistic.DaysOfStayTracker.Core.Constants;

namespace Logistic.DaysOfStayTracker.Blazor.Components;

public partial class CountryTable
{
    [Inject]
    public IMediator Mediator { get; set; } = null!;

    [Inject]
    public AppDbContext AppContext { get; set; } = null!;
    
    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    public IDialogService DialogService { get; set; } = null!;
    
    [Parameter]
    public CountrySearchRequest SearchRequest { get; set; } = new();
    
    private IPagedList<Country> _items = Constants.CreateEmptyPagedList<Country>();

    protected override Task OnInitializedAsync() => SearchAsync();

    public async Task SearchAsync()
    {
        _items = await Mediator.Send(SearchRequest);
    }

    private Task PageChanged(int page)
    {
        SearchRequest.Page = page;
        return SearchAsync();
    }
    
    private void Edit(Guid countryId) => NavigationManager.NavigateTo("country/" + countryId);

    private async Task Delete(Guid countryId)
    {
        var ok = await DialogService.ShowConfirmDialog(ConfirmDialog.DeleteMessage);
        if(!ok)
            return;
        
        var entity = await AppContext.Countries.FindAsync(countryId);
        if(entity == null)
            return;
        
        AppContext.Countries.Remove(entity);
        await AppContext.SaveChangesAsync();
        await SearchAsync();
    }
}