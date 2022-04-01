using System.Linq;
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
        // От 90 дней начинают отниматся дни
        
        // берем все поездки за 365 дней, (или 366) сумируем и отнимаем от 90 (90 - sum(day from 365))
        
        // отнимаем от сегодняшней даты 180 дней и смотрим попадает ли этот день, что водитель в это время был в европе
        //  1. если он не был в европе, то ничего не происходит
        //  2. если он был в европе, то 
        var yearAgo = request.Date.AddYears(-1);
        var dates = await DateRangesFromRangeAsync(yearAgo, request, cancellationToken);
        var sum = Sum(request, dates, yearAgo);
        var (additionalDate, addDays) = CalculateAdditionalDate(request, dates);
        var response = new CalculateRemainDaysResponse((int)(90 - sum), additionalDate, addDays);
        return response;
    }

    private static (DateOnly? AdditionalDate, int AdditionalDays) CalculateAdditionalDate(CalculateRemainDaysRequest request, List<DateRange> dates)
    {
        DateOnly? additionalDate = null;
        var addDays = 0;
        
        var halfOfYearAgo = request.Date.AddDays(-180);
        
        var halfOfYearAgoDateRange = dates.FirstOrDefault(e => e.EntryDate <= halfOfYearAgo && halfOfYearAgo <= e.ExitDate);
        if (halfOfYearAgoDateRange != null)
        {
            additionalDate = halfOfYearAgoDateRange.EntryDate.Max(halfOfYearAgo).AddDays(180);
            addDays = (int) CalculateDays(halfOfYearAgoDateRange, halfOfYearAgo, request.Date);
        }

        return (additionalDate, addDays);
    }

    private Task<List<DateRange>> DateRangesFromRangeAsync(DateOnly fromDate, CalculateRemainDaysRequest request, 
        CancellationToken cancellationToken)
    {
        return _db.DayOfStays
            .Where(r => r.DriverId == request.DriverId && r.EntryDate >= fromDate && r.ExitDate <= request.Date)
            .Select(r => new DateRange(r.EntryDate, r.ExitDate))
            .OrderBy(e => e.EntryDate)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    private static double Sum(CalculateRemainDaysRequest request, List<DateRange> dates, DateOnly yearAgo)
    {
        return dates
            .AsEnumerable()
            .AsParallel()
            .Select(r => CalculateDays(r, yearAgo, request.Date))
            .DefaultIfEmpty<double>(0)
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