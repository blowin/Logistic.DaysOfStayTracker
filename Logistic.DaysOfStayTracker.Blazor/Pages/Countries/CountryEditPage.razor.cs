using System;
using System.Threading.Tasks;
using Logistic.DaysOfStayTracker.Core.Countries;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace Logistic.DaysOfStayTracker.Blazor.Pages.Countries;

public partial class CountryEditPage
{
    [Parameter]
    public Guid? Id { get; set; }
    
    [Inject]
    private IMediator Mediator { get; set; } = null!;
    
    [Inject]
    private NavigationManager Navigation { get; set; } = null!;
    
    private CountryUpsertRequest _model = null!;
    
    protected override async Task OnInitializedAsync()
    {
        _model = Id == null ? 
            new CountryUpsertRequest() : 
            await Mediator.Send(new CountryUpsertModelGet(Id.Value));
    }

    private async Task Submit()
    {
        await Mediator.Send(_model);
        Navigation.NavigateTo("/countries");
    }
}