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
    
    private AppDbContext _db;

    public CalculateRemainDaysHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CalculateRemainDaysResponse> Handle(CalculateRemainDaysRequest request, CancellationToken cancellationToken)
    {
        const int maxInYearDays = 90;
        var ago180 = request.Date.AddDays(-CheckRangeInDays);
        var dates = await DateRangesFromRangeAsync(ago180, request, cancellationToken);
        
        var remainDay = maxInYearDays - Sum(dates);
        
        var (additionalDate, addDays, beenInEurope, additionalInEurope) = CalculateAdditionalDate(request, dates);
        if (beenInEurope)
            remainDay += additionalInEurope;
        
        var response = new CalculateRemainDaysResponse((int)remainDay, additionalDate, addDays);
        return response;
    }

    private static (DateOnly? AdditionalDate, int AdditionalDays, bool BeenInEurope, int AdditionalInEurope) CalculateAdditionalDate(CalculateRemainDaysRequest request, List<DateRange> dates)
    {
        var halfOfYearAgo = request.Date.AddDays(-CheckRangeInDays);
        
        var additionalRange = dates.FirstOrDefault(e => e.EntryDate <= halfOfYearAgo && halfOfYearAgo <= e.ExitDate);
        if (additionalRange != null)
        {
            var entryDate = additionalRange.EntryDate;
            additionalRange = additionalRange with {EntryDate = additionalRange.EntryDate.Max(halfOfYearAgo)};
            var additionalDate = additionalRange.EntryDate.AddDays(1).AddDays(CheckRangeInDays);
            var additionalInEuropeDateRange = new DateRangeValueType(entryDate, additionalRange.EntryDate);;
            return (additionalDate, additionalRange.TotalDays - 1, true, additionalInEuropeDateRange.TotalDays);   
        }

        var firstAdditionalRange = dates.FirstOrDefault(e => e.EntryDate > halfOfYearAgo);
        if (firstAdditionalRange != null)
        {
            var additionalDate = firstAdditionalRange.EntryDate.AddDays(CheckRangeInDays);
            return (additionalDate, firstAdditionalRange.TotalDays, false, 0);
        }
        
        return (null, 0, false, 0);
    }

    private Task<List<DateRange>> DateRangesFromRangeAsync(DateOnly fromDate, CalculateRemainDaysRequest request, 
        CancellationToken cancellationToken)
    {
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