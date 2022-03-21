using Logistic.DaysOfStayTracker.Blazor.Components;
using Logistic.DaysOfStayTracker.Core.Drivers;

namespace Logistic.DaysOfStayTracker.Blazor.Pages.Drivers;

public partial class DriverPage
{
    private DriverTable _table;
    
    private readonly DriverSearchRequest _searchRequest = new();
}