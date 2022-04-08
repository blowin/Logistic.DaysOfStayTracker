using Logistic.DaysOfStayTracker.Core.Extension;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Logistic.DaysOfStayTracker.Core.DayOfStays.Commands;

public record CalculateRemainDaysRequest(Guid DriverId, DateOnly Date) : IRequest<CalculateRemainDaysResponse>;

public record CalculateRemainDaysResponse(int RemainDays, DateOnly? DateOfAddDays, int AddDays);

public sealed class CalculateRemainDaysHandler 
    : IRequestHandler<CalculateRemainDaysRequest, CalculateRemainDaysResponse>
{
    private const int CheckRangeInDays = 180;
    private const int MaxDaysInYear = 90;
    
    private readonly AppDbContext _db;

    public CalculateRemainDaysHandler(AppDbContext db) => _db = db;

    public async Task<CalculateRemainDaysResponse> Handle(CalculateRemainDaysRequest request, CancellationToken cancellationToken)
    {
        var dates = await DateRangesFromRangeAsync(request, cancellationToken);
        var remainDay = MaxDaysInYear - Sum(dates);
        
        var additionalDate = CalculateAdditionalDate(request, dates);
        return additionalDate.ToResponse(remainDay);
    }

    private static AdditionalDate CalculateAdditionalDate(CalculateRemainDaysRequest request, List<DateRange> dates)
    {
        var halfOfYearAgo = request.Date.AddDays(-CheckRangeInDays);
        
        var additionalRange = dates.FirstOrDefault(e => e.EntryDate <= halfOfYearAgo && halfOfYearAgo <= e.ExitDate);
        if (additionalRange != null)
        {
            var entryDate = additionalRange.EntryDate;
            additionalRange = additionalRange with {EntryDate = additionalRange.EntryDate.Max(halfOfYearAgo)};
            var additionalDate = additionalRange.EntryDate.AddDays(1).AddDays(CheckRangeInDays);
            var additionalInEuropeDateRange = new DateRangeValueType(entryDate, additionalRange.EntryDate);
            return AdditionalDate.Europe(additionalDate, additionalRange.TotalDays - 1, additionalInEuropeDateRange.TotalDays);   
        }

        var firstAdditionalRange = dates.FirstOrDefault(e => e.EntryDate > halfOfYearAgo);
        if (firstAdditionalRange != null)
        {
            var additionalDate = firstAdditionalRange.EntryDate.AddDays(CheckRangeInDays);
            return AdditionalDate.NonEurope(additionalDate, firstAdditionalRange.TotalDays);
        }
        
        return AdditionalDate.NonEurope(null, 0);
    }
    
    private Task<List<DateRange>> DateRangesFromRangeAsync(CalculateRemainDaysRequest request, CancellationToken cancellationToken)
    {
        var fromDate = request.Date.AddDays(-CheckRangeInDays);
        return _db.DayOfStays.Where(r => r.DriverId == request.DriverId)
            .Where(r => (r.EntryDate <= request.Date && fromDate <= r.ExitDate))
            .OrderBy(e => e.EntryDate)
            .Select(r => new DateRange(r.EntryDate, r.ExitDate))
            .ToListAsync(cancellationToken: cancellationToken);
    }

    private static double Sum(List<DateRange> dates)
    {
        return dates
            .AsEnumerable()
            .AsParallel()
            .Select(r => r.TotalDays)
            .DefaultIfEmpty(0)
            .Sum();
    }
    
    private readonly record struct AdditionalDate
    {
        private AdditionalDate(DateOnly? date, int days, bool beenInEurope, int daysInEurope)
        {
            Date = date;
            Days = days;
            BeenInEurope = beenInEurope;
            DaysInEurope = daysInEurope;
        }

        public DateOnly? Date { get; init; }
        public int Days { get; init; }
        
        private bool BeenInEurope { get; init; }
        private int DaysInEurope { get; init; }
        
        public CalculateRemainDaysResponse ToResponse(double actualRemainDays)
        {
            var remainDays = (int)RemainDays(actualRemainDays);
            return new CalculateRemainDaysResponse(remainDays, Date, Days);
        }
        
        private double RemainDays(double actualRemainDays)
        {
            if (BeenInEurope)
                return actualRemainDays + DaysInEurope;
            return actualRemainDays;
        }
        
        public static AdditionalDate NonEurope(DateOnly? date, int additionalDays) =>
            new(date, additionalDays, false, 0);
        
        public static AdditionalDate Europe(DateOnly? date, int additionalDays, int inEuropeDays) =>
            new(date, additionalDays, true, inEuropeDays);
    }
}

public record DateRange(DateOnly EntryDate, DateOnly ExitDate)
{
    public int TotalDays => new DateRangeValueType(EntryDate, ExitDate).TotalDays;
}

public readonly ref struct DateRangeValueType
{
    public readonly DateOnly EntryDate;
    public readonly DateOnly ExitDate;

    public DateRangeValueType(DateOnly entryDate, DateOnly exitDate)
    {
        EntryDate = entryDate;
        ExitDate = exitDate;
    }
    
    public int TotalDays
    {
        get
        {
            var totalDays = ExitDate.Subtract(EntryDate).TotalDays;
            return (int)totalDays + 1;
        }
    }
}