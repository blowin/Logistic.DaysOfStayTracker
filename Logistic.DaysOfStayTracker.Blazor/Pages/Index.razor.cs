using Logistic.DaysOfStayTracker.Blazor.Components;
using Logistic.DaysOfStayTracker.Core.Drivers;

namespace Logistic.DaysOfStayTracker.Blazor.Pages;

public partial class Index
{
    private DriverTable _table;
    
    private readonly DriverSearchRequest _searchRequest = new();
}