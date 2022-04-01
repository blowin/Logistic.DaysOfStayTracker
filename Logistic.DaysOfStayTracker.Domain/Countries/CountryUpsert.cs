using CSharpFunctionalExtensions;
using FluentValidation;
using Logistic.DaysOfStayTracker.Core.Common;
using Logistic.DaysOfStayTracker.Core.Extension;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Logistic.DaysOfStayTracker.Core.Countries;

public class CountryUpsertRequest : IValidationRequest
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
}

public record CountryUpsertModelGet(Guid Id) : IRequest<CountryUpsertRequest>;

public sealed class CountryUpsertHandler : IValidationRequestHandler<CountryUpsertRequest>, IRequestHandler<CountryUpsertModelGet, CountryUpsertRequest>
{
    private AppDbContext _db;
    private IEnumerable<IValidator<Country>> _validators;

    public CountryUpsertHandler(AppDbContext db, IEnumerable<IValidator<Country>> validators)
    {
        _db = db;
        _validators = validators;
    }

    public async Task<Result<Unit, ICollection<string>>> Handle(CountryUpsertRequest request, CancellationToken cancellationToken)
    {
        var country = request.Id != null
            ? await _db.Countries.AsTracking().FirstAsync(e => e.Id == request.Id, cancellationToken)
            : new Country();
        
        country.Name = request.Name ?? string.Empty;
        var result = await _validators.ValidateAsync(country, cancellationToken);
        if (result.IsFailure)
            return result;
        
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