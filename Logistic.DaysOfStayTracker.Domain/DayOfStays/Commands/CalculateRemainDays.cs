using System.Text;
using Logistic.DaysOfStayTracker.Core.Extension;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Logistic.DaysOfStayTracker.Core.DayOfStays.Commands;

public record CalculateRemainDaysRequest(Guid DriverId, DateOnly Date) : IRequest<CalculateRemainDaysResponse>;

public class CalculateRemainDaysResponse : IEquatable<CalculateRemainDaysResponse>
{
    public CalculateRemainDaysResponse(int remainDays, (DateOnly? DateOfAddDays, int AddDays)[] additionDates)
    {
        RemainDays = remainDays;
        AdditionDates = additionDates;
    }

    public int RemainDays { get; }
    public (DateOnly? DateOfAddDays, int AddDays)[] AdditionDates { get; }
    
    public bool Equals(CalculateRemainDaysResponse? other)
    {
        if (ReferenceEquals(null, other)) 
            return false;
        if (ReferenceEquals(this, other)) 
            return true;
        if(AdditionDates.Length != other.AdditionDates.Length)
            return false;
        
        return RemainDays == other.RemainDays && 
               (AdditionDates.Length == 0 || AdditionDates.SequenceEqual(other.AdditionDates));
    }

    public override bool Equals(object? obj)
    {
        return obj is CalculateRemainDaysResponse crd && Equals(crd);
    }
    
    public override int GetHashCode()
    {
        if(AdditionDates.Length == 0)
            return RemainDays.GetHashCode();

        var additionalDateHashCode = AdditionDates.Aggregate(0, HashCode.Combine);
        return HashCode.Combine(RemainDays, additionalDateHashCode);
    }

    public override string ToString()
    {
        var messageBuilder = new StringBuilder(52);
        messageBuilder.AppendLine("{");
        
        messageBuilder.AppendFormat("\tRemainDays = {0}", RemainDays);
        messageBuilder.Append("\tAdditionDates = [");
        for (var index = 0; index < AdditionDates.Length; index++)
        {
            var (date, days) = AdditionDates[index];
            if(index > 0)
                messageBuilder.Append(", ");
            messageBuilder.AppendFormat("(Date = {0}, Days = {1})", date, days);
        }

        messageBuilder.AppendLine("]");
        
        messageBuilder.Append("}");
        return messageBuilder.ToString();
    }
}

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
        var additionalDates = CalculateAdditionalDates(request, dates);
        return ToResponse(additionalDates, remainDay);
    }

    private static CalculateRemainDaysResponse ToResponse(AdditionalDate[] additionalDates, double remainDay)
    {
        foreach (var additionalDate in additionalDates)
            remainDay = additionalDate.RemainDays(remainDay);

        return new CalculateRemainDaysResponse((int)remainDay, 
            additionalDates.Select(r => (r.Date, r.Days)).ToArray());
    }
    
    private static AdditionalDate[] CalculateAdditionalDates(CalculateRemainDaysRequest request, List<DateRange> dates)
    {
        var halfOfYearAgo = request.Date.AddDays(-CheckRangeInDays);
        
        var additionalRange = dates.FirstOrDefault(e => e.EntryDate <= halfOfYearAgo && halfOfYearAgo <= e.ExitDate);
        if (additionalRange != null)
        {
            var nextRange = dates.FirstOrDefault(e => e != additionalRange);
            if (nextRange == null)
                return new[]
                {
                    AdditionalDate.ForEuropeAdditionalDate(additionalRange, halfOfYearAgo)
                };
            
            return new []
            {
                AdditionalDate.ForEuropeAdditionalDate(additionalRange, halfOfYearAgo),
                AdditionalDate.ForNonEuropeAdditionalDate(nextRange), 
            };   
        }

        var nonEuropeRange = dates.Where(e => e.EntryDate > halfOfYearAgo)
            .Take(2)
            .Select(AdditionalDate.ForNonEuropeAdditionalDate)
            .ToArray();
        
        return nonEuropeRange.Length > 0 ? nonEuropeRange : new []{ AdditionalDate.Empty };
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
    
    private readonly record struct AdditionalDate(DateOnly? Date, int Days, bool BeenInEurope, int DaysInEurope)
    {
        public static AdditionalDate Empty => new(null, 0, false, 0);
        
        public double RemainDays(double actualRemainDays)
        {
            if (BeenInEurope)
                return actualRemainDays + DaysInEurope;
            return actualRemainDays;
        }
        
        public static AdditionalDate ForNonEuropeAdditionalDate(DateRange range)
        {
            var additionalDate = range.EntryDate.AddDays(CheckRangeInDays);
            return new(additionalDate, range.TotalDays, false, 0);
        }
    
        public static AdditionalDate ForEuropeAdditionalDate(DateRange additionalRange, DateOnly halfOfYearAgo)
        {
            var entryDate = additionalRange.EntryDate;
            additionalRange = additionalRange with {EntryDate = additionalRange.EntryDate.Max(halfOfYearAgo)};
            var additionalDate = additionalRange.EntryDate.AddDays(1).AddDays(CheckRangeInDays);
            var additionalInEuropeDateRange = new DateRangeValueType(entryDate, additionalRange.EntryDate);
            return new(additionalDate, additionalRange.TotalDays - 1, true, additionalInEuropeDateRange.TotalDays);   
        }
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