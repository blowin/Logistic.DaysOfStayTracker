using CSharpFunctionalExtensions;
using FluentValidation;
using Logistic.DaysOfStayTracker.Core.Common;
using Logistic.DaysOfStayTracker.Core.Extension;

namespace Logistic.DaysOfStayTracker.Core.DayOfStays.Commands;

public record DayOfStayCreateRequest(Guid DriverId, DateOnly EntryDate, DateOnly ExitDate) : IValidationRequest<DayOfStay>;

public sealed class DayOfStayCreateHandler : IValidationRequestHandler<DayOfStayCreateRequest, DayOfStay>
{
    private readonly IEnumerable<IValidator<DayOfStay>> _validators;

    public DayOfStayCreateHandler(IEnumerable<IValidator<DayOfStay>> validators)
    {
        _validators = validators;
    }

    public async Task<Result<DayOfStay, ICollection<string>>> Handle(DayOfStayCreateRequest request, CancellationToken cancellationToken)
    {
        var createEntity = new DayOfStay
        {
            DriverId = request.DriverId,
            EntryDate = request.EntryDate,
            ExitDate = request.ExitDate
        };

        var result = await _validators.ValidateAsync(createEntity, cancellationToken);
        return result.Map(_ => createEntity);
    }
}