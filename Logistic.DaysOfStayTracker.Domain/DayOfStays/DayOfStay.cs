using Logistic.DaysOfStayTracker.Core.Common;
using Logistic.DaysOfStayTracker.Core.Countries;

namespace Logistic.DaysOfStayTracker.Core.DayOfStays;

public class DayOfStay : Entity
{
    public DateOnly EntryDate { get; internal set; }
    public DateOnly ExitDate { get; internal set; }
    
    public Guid DriverId { get; internal set; }
    
    public Guid EntryCountryId { get; internal set; }
    public Country? EntryCountry { get; internal set; }
    
    public Guid ExitCountryId { get; internal set; }
    public Country? ExitCountry { get; internal set; }
}