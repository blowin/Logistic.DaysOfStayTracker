using Logistic.DaysOfStayTracker.Core.Database;
using MediatR;
using X.PagedList;

namespace Logistic.DaysOfStayTracker.Core.DayOfStays;

public record DayOfStaySearchRequest : IRequest<IPagedList<DayOfStaySearchResponse>>
{
    public int Page { get; set; } = 1;
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
    public Guid? DriverId { get; set; }
}

public class DayOfStaySearchResponse : DayOfStay
{
    public string DriverFullName { get; set; } = string.Empty;
}

public sealed class DayOfStaySearchHandler : IRequestHandler<DayOfStaySearchRequest, IPagedList<DayOfStaySearchResponse>>
{
    private readonly AppDbContext _dbContext;

    public DayOfStaySearchHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<IPagedList<DayOfStaySearchResponse>> Handle(DayOfStaySearchRequest request, CancellationToken cancellationToken)
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

        return query.Join(_dbContext.Drivers, stay => stay.DriverId, driver => driver.Id, (stay, driver) => new DayOfStaySearchResponse
            {
                Id = stay.Id,
                Start = stay.Start,
                End = stay.End,
                DriverId = stay.DriverId,
                DriverFullName = driver.FirstName + " " + driver.LastName
            })
            .ToPagedListAsync(request.Page, Constants.DefaultPageSize, cancellationToken);
    }
}