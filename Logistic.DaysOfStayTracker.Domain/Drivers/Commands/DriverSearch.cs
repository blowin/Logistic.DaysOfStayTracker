using Logistic.DaysOfStayTracker.Core.Extension;
using MediatR;
using X.PagedList;

namespace Logistic.DaysOfStayTracker.Core.Drivers.Commands;

public class DriverSearchRequest : IRequest<IPagedList<Driver>>
{
    public int Page { get; set; } = 1;
    public string? FullName { get; set; }
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
            .OrderBy(e => e.LastName).ThenBy(e => e.FirstName);

        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            var words = request.FullName.Trim().Split(' ');
            switch (words.Length)
            {
                case 0 or 1:
                    query = query.Where(e => e.FirstName.Contains(request.FullName) || e.LastName.Contains(request.FullName));
                    break;
                case 2:
                    var v1 = words[0];
                    var v2 = words[1];
                    query = query.Where(e => (e.FirstName.Contains(v1) && e.LastName.Contains(v2)) || (e.FirstName.Contains(v2) && e.LastName.Contains(v1)));
                    break;
                default:
                    query = query.Take(0);
                    break;
            }
        }
        
        return query.ToPagedListAsync(request.Page, cancellationToken);
    }
}