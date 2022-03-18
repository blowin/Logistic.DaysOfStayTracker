namespace Logistic.DaysOfStayTracker.Core.DayOfStays;

public class DayOfStay : Entity
{
    public DateOnly Start { get; set; }
    public DateOnly End { get; set; }
    
    public Guid DriverId { get; set; }
}