using FluentValidation;

namespace Logistic.DaysOfStayTracker.Core.Drivers;

public class DriverValidator : AbstractValidator<Driver>
{
    public DriverValidator()
    {
        RuleFor(e => e.FirstName)
            .NotEmpty()
            .MaximumLength(32)
            .WithName("Имя");
        
        RuleFor(e => e.LastName)
            .NotEmpty()
            .MaximumLength(32)
            .WithName("Фамилия");
    }
}