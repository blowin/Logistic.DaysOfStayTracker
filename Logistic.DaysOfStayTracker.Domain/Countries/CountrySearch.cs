using Logistic.DaysOfStayTracker.Core.Extension;
using MediatR;
using X.PagedList;

namespace Logistic.DaysOfStayTracker.Core.Countries;

public record CountrySearchRequest : IRequest<IPagedList<Country>>
{
    public int Page { get; set; } = 1;
    public string? Name { get; set; }
}

public sealed class CountrySearchHandler : IRequestHandler<CountrySearchRequest, IPagedList<Country>>
{
    private AppDbContext _context;

    public CountrySearchHandler(AppDbContext context)
    {
        _context = context;
    }

    public Task<IPagedList<Country>> Handle(CountrySearchRequest request, CancellationToken cancellationToken)
    {
        IQueryable<Country> query = _context.Countries.OrderBy(e => e.Name);

        if (!string.IsNullOrWhiteSpace(request.Name))
            query = query.Where(e => e.Name.Contains(request.Name));

        return query.ToPagedListAsync(request.Page, cancellationToken);
    }
}