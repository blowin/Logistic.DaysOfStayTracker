using Logistic.DaysOfStayTracker.Blazor.Components;
using Logistic.DaysOfStayTracker.Core.Drivers.Commands;
using Microsoft.AspNetCore.Components;

namespace Logistic.DaysOfStayTracker.Blazor.Pages.Drivers;

public partial class DriverPage
{
    private DriverTable _table = null!;
    
    private readonly DriverSearchRequest _searchRequest = new();

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private void Create() => NavigationManager.NavigateTo("driver/");
}