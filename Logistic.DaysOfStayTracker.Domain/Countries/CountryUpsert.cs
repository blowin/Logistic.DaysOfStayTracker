using Logistic.DaysOfStayTracker.Core.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Logistic.DaysOfStayTracker.Core.Countries;

public class CountryUpsertRequest : IRequest
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
}

public record CountryUpsertModelGet(Guid Id) : IRequest<CountryUpsertRequest>;

public sealed class CountryUpsertHandler : IRequestHandler<CountryUpsertRequest>, IRequestHandler<CountryUpsertModelGet, CountryUpsertRequest>
{
    private AppDbContext _db;

    public CountryUpsertHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Unit> Handle(CountryUpsertRequest request, CancellationToken cancellationToken)
    {
        // TODO validate
        var country = request.Id != null
            ? await _db.Countries.AsTracking().FirstAsync(e => e.Id == request.Id, cancellationToken)
            : new Country();
        
        country.Name = request.Name ?? string.Empty;
        
        if (request.Id == null)
        {
            await _db.Countries.AddAsync(country, cancellationToken);
        }
        else
        {
            _db.Countries.Update(country);   
        }
        
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    public async Task<CountryUpsertRequest> Handle(CountryUpsertModelGet request, CancellationToken cancellationToken)
    {
        var country = await _db.Countries.FirstAsync(r => r.Id == request.Id, cancellationToken);
        return new CountryUpsertRequest
        {
            Id = country.Id,
            Name = country.Name,
        };
    }
}