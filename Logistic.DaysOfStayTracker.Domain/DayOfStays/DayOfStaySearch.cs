using CSharpFunctionalExtensions;
using Logistic.DaysOfStayTracker.Core.Common;
using Microsoft.EntityFrameworkCore;

namespace Logistic.DaysOfStayTracker.Core.DayOfStays;

public record DayOfStaySearchRequest : IValidationRequest<List<DayOfStay>>
{
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
    public Guid? DriverId { get; set; }
    public DateTime? Year { get; set; }
}

public sealed class DayOfStaySearchHandler : IValidationRequestHandler<DayOfStaySearchRequest, List<DayOfStay>>
{
    private readonly AppDbContext _dbContext;

    public DayOfStaySearchHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<DayOfStay>, ICollection<string>>> Handle(DayOfStaySearchRequest request, CancellationToken cancellationToken)
    {
        if (request.End == null && request.Start == null && request.Year == null)
        {
            return Result.Failure<List<DayOfStay>, ICollection<string>>(new List<string>
            {
                "Необходимо указать как минимум один фильтр"
            });
        }

        IQueryable<DayOfStay> query = _dbContext.DayOfStays
            .OrderByDescending(e => e.EntryDate)
            .ThenByDescending(e => e.ExitDate);

        if (request.Start.HasValue)
        {
            var start = DateOnly.FromDateTime(request.Start.Value);
            query = query.Where(e => e.EntryDate <= start && e.ExitDate >= start);
        }
        
        if (request.End.HasValue)
        {
            var end = DateOnly.FromDateTime(request.End.Value);
            query = query.Where(e => e.EntryDate <= end && e.ExitDate >= end);
        }

        if (request.Year.HasValue)
        {
            var year = request.Year.Value.Year;
            query = query.Where(e => e.EntryDate.Year == year || e.ExitDate.Year == year);
        }

        if (request.DriverId.HasValue)
        {
            var driverId = request.DriverId.Value;
            query = query.Where(e => e.DriverId == driverId);
        }

        return await query.ToListAsync(cancellationToken);
    }
}