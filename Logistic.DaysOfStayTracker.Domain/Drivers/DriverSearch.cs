using Logistic.DaysOfStayTracker.Core.Extension;
using MediatR;
using X.PagedList;

namespace Logistic.DaysOfStayTracker.Core.Drivers;

public class DriverSearchRequest : IRequest<IPagedList<Driver>>
{
    public int Page { get; set; } = 1;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public class DriverSearchHandler : IRequestHandler<DriverSearchRequest, IPagedList<Driver>>
{
    private AppDbContext _dbContext;

    public DriverSearchHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<IPagedList<Driver>> Handle(DriverSearchRequest request, CancellationToken cancellationToken)
    {
        IQueryable<Driver> query = _dbContext.Drivers
            .OrderBy(e => e.FirstName).ThenBy(e => e.LastName);

        if (!string.IsNullOrWhiteSpace(request.FirstName))
            query = query.Where(e => e.FirstName.Contains(request.FirstName));

        if (!string.IsNullOrWhiteSpace(request.LastName))
            query = query.Where(e => e.LastName.Contains(request.LastName));

        return query.ToPagedListAsync(request.Page, cancellationToken);
    }
}