using Logistic.DaysOfStayTracker.Core.Common;

namespace Logistic.DaysOfStayTracker.Core.Drivers;

public class Driver : Entity
{
    public string FirstName { get; internal set; } = string.Empty;
    public string LastName { get; internal set; } = string.Empty;
    public DateOnly? VisaExpiryDate { get; internal set; }
    
    internal Driver(){}
}