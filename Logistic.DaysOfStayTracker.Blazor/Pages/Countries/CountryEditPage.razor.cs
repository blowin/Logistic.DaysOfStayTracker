using System;
using System.Threading.Tasks;
using Logistic.DaysOfStayTracker.Core.Countries;
using Logistic.DaysOfStayTracker.Core.Database;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace Logistic.DaysOfStayTracker.Blazor.Pages.Countries;

public partial class CountryEditPage
{
    [Parameter]
    public Guid? Id { get; set; }
    
    [Inject]
    private AppDbContext AppContext { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;
    
    private Country _model = null!;
    
    protected override async Task OnInitializedAsync()
    {
        Country? country = null;
        if (Id != null)
        {
            var id = Id.Value;
            country = await AppContext.Countries.FirstOrDefaultAsync(r => r.Id == id);
        }

        country ??= new Country();
        _model = country;
    }

    private async Task Submit()
    {
        // TODO validate
        if (_model.Id == Guid.Empty)
        {
            await AppContext.Countries.AddAsync(_model);
        }
        else
        {
            var updateDriver = await AppContext.Countries.AsTracking().FirstAsync(r => r.Id == _model.Id);
            updateDriver.Name = _model.Name;
            updateDriver.IsEuropeanUnion = _model.IsEuropeanUnion;
            AppContext.Countries.Update(updateDriver);
        }

        await AppContext.SaveChangesAsync();
        Navigation.NavigateTo("/countries");
    }
}