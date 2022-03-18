using Logistic.DaysOfStayTracker.Blazor.Components;
using Logistic.DaysOfStayTracker.Core.Drivers;

namespace Logistic.DaysOfStayTracker.Blazor.Pages;

public partial class DriverTablePage
{
    private DriverTable _table;
    
    private readonly DriverSearchRequest _searchRequest = new();
}