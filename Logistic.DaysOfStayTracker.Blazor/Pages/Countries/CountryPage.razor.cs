using Logistic.DaysOfStayTracker.Blazor.Components;
using Logistic.DaysOfStayTracker.Core.Countries;
using Logistic.DaysOfStayTracker.Core.Countries.Commands;
using Microsoft.AspNetCore.Components;

namespace Logistic.DaysOfStayTracker.Blazor.Pages.Countries;

public partial class CountryPage
{
    private CountryTable _table = null!;
    
    private readonly CountrySearchRequest _searchRequest = new();

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;
    
    private void Create() => NavigationManager.NavigateTo("country/");
}