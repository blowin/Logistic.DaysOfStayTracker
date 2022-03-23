using Logistic.DaysOfStayTracker.Core.Common;

namespace Logistic.DaysOfStayTracker.Core.Countries;

public class Country : Entity
{
    public string Name { get; internal set; } = string.Empty;
}