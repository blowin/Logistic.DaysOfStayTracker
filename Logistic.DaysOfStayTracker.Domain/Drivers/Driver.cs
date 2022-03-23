using Logistic.DaysOfStayTracker.Core.Common;

namespace Logistic.DaysOfStayTracker.Core.Drivers;

public class Driver : Entity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}