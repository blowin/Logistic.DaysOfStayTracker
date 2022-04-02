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
        var ago180 = request.Date.AddDays(-180);
        var dates = await DateRangesFromRangeAsync(ago180, request, cancellationToken);
        
        var remainDay = maxInYearDays - Sum(request, dates, ago180);
        
        var (additionalDate, addDays, beenInEurope) = CalculateAdditionalDate(request, dates);
        if (beenInEurope)
            remainDay += 1;
        
        var response = new CalculateRemainDaysResponse((int)remainDay, additionalDate, addDays);
        return response;
    }

    private static (DateOnly? AdditionalDate, int AdditionalDays, bool BeenInEurope) CalculateAdditionalDate(CalculateRemainDaysRequest request, List<DateRange> dates)
    {
        const int checkRange = 180;
        
        var halfOfYearAgo = request.Date.AddDays(-checkRange);
        
        var additionalRange = dates.FirstOrDefault(e => e.EntryDate <= halfOfYearAgo && halfOfYearAgo <= e.ExitDate);
        if (additionalRange != null)
        {
            additionalRange = additionalRange with {EntryDate = additionalRange.EntryDate.Max(halfOfYearAgo)};
            var additionalDate = additionalRange.EntryDate.AddDays(checkRange);
            return (additionalDate, additionalRange.TotalDays - 1, true);   
        }

        var firstAdditionalRange = dates.FirstOrDefault(e => e.EntryDate > halfOfYearAgo);
        if (firstAdditionalRange != null)
        {
            var additionalDate = firstAdditionalRange.EntryDate.AddDays(checkRange);
            return (additionalDate, firstAdditionalRange.TotalDays, false);
        }
        
        return (null, 0, false);
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

    private static double Sum(CalculateRemainDaysRequest request, List<DateRange> dates, DateOnly fromDate)
    {
        var dd = dates.Select(e => e.TotalDays).ToList();
        return dates
            .AsEnumerable()
            .AsParallel()
            .Select(r => r.TotalDays)
            .DefaultIfEmpty(0)
            .Sum();
    }

    private record DateRange(DateOnly EntryDate, DateOnly ExitDate)
    {
        public int TotalDays
        {
            get
            {
                var totalDays = ExitDate.Subtract(EntryDate).TotalDays;
                return (int)totalDays + 1;
            }
        }
    }
}