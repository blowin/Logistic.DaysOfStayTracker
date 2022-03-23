using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Logistic.DaysOfStayTracker.Core.Countries;

public class CountryValidator : AbstractValidator<Country>
{
    private readonly AppDbContext _db;
    
    public CountryValidator(AppDbContext db)
    {
        _db = db;

        CascadeMode = CascadeMode.Stop;
        
        RuleFor(e => e.Name)
            .NotEmpty().WithName("Название")
            .MaximumLength(64)
            .MustAsync(BeUnique).WithMessage("'Название' должно быть уникальным");
    }

    private Task<bool> BeUnique(Country country, string countryName, CancellationToken cancellationToken)
    {
        IQueryable<Country> countries = _db.Countries;
        if (country.Id != Guid.Empty)
            countries = countries.Where(e => e.Id != country.Id);

        return countries.AllAsync(e => e.Name != countryName, cancellationToken);
    }
}