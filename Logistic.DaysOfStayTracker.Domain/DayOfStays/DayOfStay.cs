using CSharpFunctionalExtensions;
using Logistic.DaysOfStayTracker.Core.Countries;
using Entity = Logistic.DaysOfStayTracker.Core.Common.Entity;

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
    
    internal DayOfStay(){}

    public static Result<DayOfStay> Create(Guid driverId, 
        Guid entryCountryId, DateOnly entryDate, 
        Guid exitCountryId, DateOnly exitDate)
    {
        if (entryCountryId == exitCountryId)
            return Result.Failure<DayOfStay>("Страны не могу совпадать");

        if (entryDate > exitDate)
            return Result.Failure<DayOfStay>("Дата въезда не может быть позже даты выезда");

        return Result.Success(new DayOfStay
        {
            DriverId = driverId,

            EntryCountryId = entryCountryId,
            EntryDate = entryDate,

            ExitCountryId = exitCountryId,
            ExitDate = exitDate
        });
    }
}