using Logistic.DaysOfStayTracker.Core.Countries;

namespace Logistic.DaysOfStayTracker.Core.DayOfStays;

public class DayOfStay : Entity
{
    public DateOnly EntryDate { get; set; }
    public DateOnly ExitDate { get; set; }
    
    public Guid DriverId { get; set; }
    
    public Guid EntryCountryId { get; set; }
    public Country? EntryCountry { get; set; }
    
    public Guid ExitCountryId { get; set; }
    public Country? ExitCountry { get; set; }
}