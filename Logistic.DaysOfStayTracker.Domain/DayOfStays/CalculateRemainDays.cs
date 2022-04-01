using Logistic.DaysOfStayTracker.Core.Extension;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Logistic.DaysOfStayTracker.Core.DayOfStays;

public record CalculateRemainDaysRequest(Guid DriverId, DateOnly Date) : IRequest<CalculateRemainDaysResponse>;

public record CalculateRemainDaysResponse(int RemainDays, DateOnly? DateOfAddDays, int AddDays);

public sealed class CalculateRemainDaysHandler 
    : IRequestHandler<CalculateRemainDaysRequest, CalculateRemainDaysResponse>
{
    private AppDbContext _db;

    public CalculateRemainDaysHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CalculateRemainDaysResponse> Handle(CalculateRemainDaysRequest request, CancellationToken cancellationToken)
    {
        const int maxInYearDays = 90;
        
        var yearAgo = request.Date.AddYears(-1);
        var dates = await DateRangesFromRangeAsync(yearAgo, request, cancellationToken);
        var sum = Sum(request, dates, yearAgo);
        var (additionalDate, addDays) = CalculateAdditionalDate(request, dates);
        var response = new CalculateRemainDaysResponse((int)(maxInYearDays - sum), additionalDate, addDays);
        return response;
    }

    private static (DateOnly? AdditionalDate, int AdditionalDays) CalculateAdditionalDate(CalculateRemainDaysRequest request, List<DateRange> dates)
    {
        const int checkRange = 180;
        
        var halfOfYearAgo = request.Date.AddDays(-checkRange);
        
        var halfOfYearAgoDateRange = dates.FirstOrDefault(e => e.EntryDate <= halfOfYearAgo && halfOfYearAgo <= e.ExitDate);
        if (halfOfYearAgoDateRange == null) 
            return (null, 0);
        
        var additionalDate = halfOfYearAgoDateRange.EntryDate.Max(halfOfYearAgo).AddDays(checkRange);
        var addDays = (int) CalculateDays(halfOfYearAgoDateRange, halfOfYearAgo, request.Date);
        return (additionalDate, addDays);
    }

    private Task<List<DateRange>> DateRangesFromRangeAsync(DateOnly fromDate, CalculateRemainDaysRequest request, 
        CancellationToken cancellationToken)
    {
        return _db.DayOfStays
            .Where(r => r.DriverId == request.DriverId && r.EntryDate >= fromDate && r.ExitDate <= request.Date)
            .OrderBy(e => e.EntryDate)
            .Select(r => new DateRange(r.EntryDate, r.ExitDate))
            .ToListAsync(cancellationToken: cancellationToken);
    }

    private static double Sum(CalculateRemainDaysRequest request, List<DateRange> dates, DateOnly yearAgo)
    {
        return dates
            .AsEnumerable()
            .AsParallel()
            .Select(r => CalculateDays(r, yearAgo, request.Date))
            .DefaultIfEmpty(0)
            .Sum();
    }

    private static double CalculateDays(DateRange range, DateOnly minDateTime, DateOnly maxDateTime)
    {
        var exitDate = range.ExitDate.Min(maxDateTime);
        var entryDate = range.EntryDate.Max(minDateTime);
        var totalDays = exitDate.Subtract(entryDate).TotalDays;
        return totalDays + 1;
    }
    
    private record DateRange(DateOnly EntryDate, DateOnly ExitDate);
}