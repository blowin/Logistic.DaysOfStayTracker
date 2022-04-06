using Entity = Logistic.DaysOfStayTracker.Core.Common.Entity;

namespace Logistic.DaysOfStayTracker.Core.DayOfStays;

public class DayOfStay : Entity
{
    public DateOnly EntryDate { get; internal set; }
    public DateOnly ExitDate { get; internal set; }
    
    public Guid DriverId { get; internal set; }
    
    internal DayOfStay(){}
}