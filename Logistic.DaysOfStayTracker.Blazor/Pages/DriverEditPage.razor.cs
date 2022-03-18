using System;
using System.Threading.Tasks;
using Logistic.DaysOfStayTracker.Core.Database;
using Logistic.DaysOfStayTracker.Core.Drivers;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace Logistic.DaysOfStayTracker.Blazor.Pages;

public partial class DriverEditPage
{
    [Parameter]
    public Guid? Id { get; set; }

    [Inject]
    private AppDbContext AppContext { get; set; } = null!;
    
    private Driver _model = null!;

    protected override async Task OnInitializedAsync()
    {
        Driver? driver = null;
        if (Id != null)
        {
            var id = Id.Value;
            driver = await AppContext.Drivers.FirstOrDefaultAsync(r => r.Id == id);
        }

        driver ??= new Driver();
        _model = driver;
    }

    private async Task Submit()
    {
        // TODO validate
        if (_model.Id == Guid.Empty)
        {
            await AppContext.Drivers.AddAsync(_model);
        }
        else
        {
            var updateDriver = await AppContext.Drivers.AsTracking().FirstAsync(r => r.Id == _model.Id);
            updateDriver.FirstName = _model.FirstName;
            updateDriver.LastName = _model.LastName;
            AppContext.Drivers.Update(updateDriver);
        }

        await AppContext.SaveChangesAsync();
    }
}