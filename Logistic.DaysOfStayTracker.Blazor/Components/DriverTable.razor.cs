using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Logistic.DaysOfStayTracker.Blazor.Extension;
using Logistic.DaysOfStayTracker.Core;
using Logistic.DaysOfStayTracker.Core.Drivers;
using Logistic.DaysOfStayTracker.Core.Drivers.Commands;
using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using X.PagedList;

namespace Logistic.DaysOfStayTracker.Blazor.Components;

public partial class DriverTable
{
    [Inject]
    private IMediator Mediator { get; set; } = null!;
    
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;
    
    [Inject]
    public IDialogService DialogService { get; set; } = null!;
    
    [Parameter]
    public DriverSearchRequest SearchRequest { get; set; } = new();
    
    private IPagedList<Driver> _items = Constants.CreateEmptyPagedList<Driver>();
    private ICollection<string> _errors = Array.Empty<string>();

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
        _errors = Array.Empty<string>();
        _items = await Mediator.Send(SearchRequest);
    }

    private void Edit(Guid id) => NavigationManager.NavigateTo("driver/" + id);

    private async Task Delete(Guid driverId)
    {
        var ok = await DialogService.ShowConfirmDialog(ConfirmDialog.DeleteMessage);
        if(!ok)
            return;
        
        var result = await Mediator.Send(new DriverDeleteRequest(driverId));
        if (result.IsFailure)
        {
            _errors = result.Error;
            return;
        }

        await SearchAsync();
    }
}