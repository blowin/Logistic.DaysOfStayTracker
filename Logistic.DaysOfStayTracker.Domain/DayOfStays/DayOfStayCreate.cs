using CSharpFunctionalExtensions;
using FluentValidation;
using Logistic.DaysOfStayTracker.Core.Common;
using Logistic.DaysOfStayTracker.Core.Extension;

namespace Logistic.DaysOfStayTracker.Core.DayOfStays;

public record DayOfStayCreateRequest(Guid DriverId,
    Guid EntryCountryId, DateOnly EntryDate,
    Guid ExitCountryId, DateOnly ExitDate) : IValidationRequest<DayOfStay>;

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
            ExitDate = request.ExitDate,
            EntryCountryId = request.EntryCountryId,
            ExitCountryId = request.ExitCountryId
        };

        var result = await _validators.ValidateAsync(createEntity, cancellationToken);
        return result.Map(_ => createEntity);
    }
}