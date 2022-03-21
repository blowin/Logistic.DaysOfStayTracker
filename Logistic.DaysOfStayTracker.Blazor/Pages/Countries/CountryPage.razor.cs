using Logistic.DaysOfStayTracker.Blazor.Components;
using Logistic.DaysOfStayTracker.Core.Countries;

namespace Logistic.DaysOfStayTracker.Blazor.Pages.Countries;

public partial class CountryPage
{
    private CountryTable _table;
    
    private readonly CountrySearchRequest _searchRequest = new();
}