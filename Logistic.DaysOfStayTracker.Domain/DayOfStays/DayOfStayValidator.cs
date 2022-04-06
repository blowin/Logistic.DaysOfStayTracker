using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Logistic.DaysOfStayTracker.Core.DayOfStays;

public class DayOfStayValidator : AbstractValidator<DayOfStay>
{
    private readonly AppDbContext _db;
    
    public DayOfStayValidator(AppDbContext db)
    {
        _db = db;

        RuleFor(e => e).CustomAsync(ValidateDayOfStay);
    }

    private async Task ValidateDayOfStay(DayOfStay entity, ValidationContext<DayOfStay> context, CancellationToken cancellationToken)
    {
        if (entity.EntryDate > entity.ExitDate)
        {
            context.AddFailure("Дата въезда не может быть позже даты выезда");
            return;
        }
        
        if(entity.DriverId == Guid.Empty)
            return;

        var intersectRange = await _db.DayOfStays.FirstOrDefaultAsync(r =>
            r.DriverId == entity.DriverId && 
            r.Id != entity.Id && 
            r.EntryDate <= entity.EntryDate && 
            r.ExitDate >= entity.ExitDate, cancellationToken: cancellationToken);

        if (intersectRange != null)
        {
            context.AddFailure($"Данный диапазон пересекается с диапазоном {intersectRange.EntryDate}-{intersectRange.ExitDate}");
            return;
        }
    }
}