using Ardalis.SmartEnum;

namespace Logistic.DaysOfStayTracker.Core;

public class SearchBool : SmartEnum<SearchBool>
{
    public static readonly SearchBool All = new("Все", 0, null);
    public static readonly SearchBool Yes = new("Да", 1, true);
    public static readonly SearchBool No = new("Нет", 2, false);
    
    public bool? BoolValue { get; }
    
    private SearchBool(string name, int value, bool? boolValue) : base(name, value)
    {
        BoolValue = boolValue;
    }
}