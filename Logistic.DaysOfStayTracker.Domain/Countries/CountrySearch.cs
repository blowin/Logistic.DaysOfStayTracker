using Logistic.DaysOfStayTracker.Core.Database;
using Logistic.DaysOfStayTracker.Core.Extension;
using MediatR;
using X.PagedList;

namespace Logistic.DaysOfStayTracker.Core.Countries;

public record CountrySearchRequest : IRequest<IPagedList<Country>>
{
    public int Page { get; set; } = 1;
    public string? Name { get; set; }
    
    public SearchBool IsEuropeanUnion { get; set; } = SearchBool.All;
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
        IQueryable<Country> query = _context.Countries;

        if (!string.IsNullOrWhiteSpace(request.Name))
            query = query.Where(e => e.Name.Contains(request.Name));

        if (request.IsEuropeanUnion.BoolValue.HasValue)
        {
            var isEuropeanUnion = request.IsEuropeanUnion.BoolValue.Value;
            query = query.Where(e => e.IsEuropeanUnion == isEuropeanUnion);
        }

        return query.ToPagedListAsync(request.Page, cancellationToken);
    }
}