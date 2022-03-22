using Logistic.DaysOfStayTracker.Blazor.Components;
using Logistic.DaysOfStayTracker.Core.Countries;
using Microsoft.AspNetCore.Components;

namespace Logistic.DaysOfStayTracker.Blazor.Pages.Countries;

public partial class CountryPage
{
    private CountryTable _table;
    
    private readonly CountrySearchRequest _searchRequest = new();

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;
    
    private void Create() => NavigationManager.NavigateTo("country/");
}