namespace Logistic.DaysOfStayTracker.Core.Countries;

public class Country : Entity
{
    public string Name { get; set; } = string.Empty;
    public bool IsEuropeanUnion { get; set; }
}