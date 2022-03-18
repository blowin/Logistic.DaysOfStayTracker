using Logistic.DaysOfStayTracker.Core.Database;
using MediatR;
using X.PagedList;

namespace Logistic.DaysOfStayTracker.Core.DayOfStays;

public record DayOfStaySearchRequest : IRequest<IPagedList<DayOfStay>>
{
    public int Page { get; set; } = 1;
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
    public Guid? DriverId { get; set; }
    public DateTime? Year { get; set; }
}

public sealed class DayOfStaySearchHandler : IRequestHandler<DayOfStaySearchRequest, IPagedList<DayOfStay>>
{
    private readonly AppDbContext _dbContext;

    public DayOfStaySearchHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<IPagedList<DayOfStay>> Handle(DayOfStaySearchRequest request, CancellationToken cancellationToken)
    {
        IQueryable<DayOfStay> query = _dbContext.DayOfStays
            .OrderByDescending(e => e.Start)
            .ThenByDescending(e => e.End);

        if (request.Start.HasValue)
        {
            var start = DateOnly.FromDateTime(request.Start.Value);
            query = query.Where(e => e.Start <= start && e.End >= start);
        }
        
        if (request.End.HasValue)
        {
            var end = DateOnly.FromDateTime(request.End.Value);
            query = query.Where(e => e.Start <= end && e.End >= end);
        }

        if (request.Year.HasValue)
        {
            var year = request.Year.Value.Year;
            query = query.Where(e => e.Start.Year == year || e.End.Year == year);
        }

        if (request.DriverId.HasValue)
        {
            var driverId = request.DriverId.Value;
            query = query.Where(e => e.DriverId == driverId);
        }

        return query.ToPagedListAsync(request.Page, Constants.DefaultPageSize, cancellationToken);
    }
}