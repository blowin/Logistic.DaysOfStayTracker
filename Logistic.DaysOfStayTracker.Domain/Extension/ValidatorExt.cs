using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;

namespace Logistic.DaysOfStayTracker.Core.Extension;

public static class ValidatorExt
{
    public static async Task<Result<Unit, ICollection<string>>> ValidateAsync<T>(this IEnumerable<IValidator<T>> self, T entity, CancellationToken cancellationToken)
    {
        foreach (var validator in self)
        {
            var result = await validator.ValidateAsync(entity, cancellationToken);
            if (result.IsValid) 
                continue;
            
            return result.Errors.Select(e => e.ErrorMessage).ToList();
        }

        return Unit.Value;
    }
}