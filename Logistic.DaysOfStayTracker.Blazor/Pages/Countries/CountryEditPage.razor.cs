using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Logistic.DaysOfStayTracker.Core.Countries;
using Logistic.DaysOfStayTracker.Core.Countries.Commands;
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

    private ICollection<string> _errors = Array.Empty<string>();

    protected override async Task OnInitializedAsync()
    {
        _model = Id == null ? 
            new CountryUpsertRequest() : 
            await Mediator.Send(new CountryUpsertModelGet(Id.Value));
    }

    private async Task Submit()
    {
        _errors = Array.Empty<string>();
        
        var result = await Mediator.Send(_model);
        if (result.IsFailure)
        {
            _errors = result.Error;
            return;
        }
        
        Navigation.NavigateTo("/countries");
    }
}