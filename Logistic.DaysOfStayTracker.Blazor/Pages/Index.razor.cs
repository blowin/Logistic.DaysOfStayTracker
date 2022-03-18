using Logistic.DaysOfStayTracker.Blazor.Components;
using Logistic.DaysOfStayTracker.Core.DayOfStays;
using MudBlazor;

namespace Logistic.DaysOfStayTracker.Blazor.Pages;

public partial class Index
{
    private DayOfStayTable _table = null!;
    private MudDatePicker _pickerStart = null!;
    private MudDatePicker _pickerEnd = null!;
    
    private DayOfStaySearchRequest _searchRequest = new();
}