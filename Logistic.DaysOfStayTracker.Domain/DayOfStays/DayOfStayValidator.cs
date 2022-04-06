using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Logistic.DaysOfStayTracker.Core.DayOfStays;

public record struct DayOfStayValidateDetail(DayOfStay DayOfStay, 
    ICollection<Guid> ExcludeEntities,
    ICollection<DayOfStay> AdditionalDayOfStays);

public class DayOfStayValidator : AbstractValidator<DayOfStayValidateDetail>
{
    private readonly AppDbContext _db;
    
    public DayOfStayValidator(AppDbContext db)
    {
        _db = db;

        RuleFor(e => e).CustomAsync(ValidateDayOfStay);
    }

    private async Task ValidateDayOfStay(DayOfStayValidateDetail entity, ValidationContext<DayOfStayValidateDetail> context,
        CancellationToken cancellationToken)
    {
        if (entity.DayOfStay.EntryDate > entity.DayOfStay.ExitDate)
        {
            context.AddFailure("Дата въезда не может быть позже даты выезда");
            return;
        }

        if (entity.DayOfStay.DriverId != Guid.Empty)
        {
            IQueryable<DayOfStay> dayOfStays = _db.DayOfStays;
            if (entity.ExcludeEntities.Count > 0)
            {
                var ex = entity.ExcludeEntities;
                dayOfStays = dayOfStays.Where(r => !ex.Contains(r.Id));
            }
            var intersectRange = await dayOfStays
                .FirstOrDefaultAsync(r =>
                    r.DriverId == entity.DayOfStay.DriverId && 
                    r.Id != entity.DayOfStay.Id && 
                    r.EntryDate <= entity.DayOfStay.EntryDate && 
                    r.ExitDate >= entity.DayOfStay.ExitDate, cancellationToken: cancellationToken);

            if (intersectRange != null)
            {
                AddInvalidRange(intersectRange);
                return;
            }   
        }

        if (entity.AdditionalDayOfStays.Count > 0)
        {
            var intersectRange = entity.AdditionalDayOfStays
                .FirstOrDefault(r => r.EntryDate <= entity.DayOfStay.EntryDate && r.ExitDate >= entity.DayOfStay.ExitDate);

            if (intersectRange != null)
            {
                AddInvalidRange(intersectRange);
                return;
            }   
        }

        void AddInvalidRange(DayOfStay range)
        {
            context.AddFailure($"Данный диапазон пересекается с диапазоном {range.EntryDate}-{range.ExitDate}");
        }
    }
}