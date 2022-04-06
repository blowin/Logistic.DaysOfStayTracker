using CSharpFunctionalExtensions;
using FluentValidation;
using Logistic.DaysOfStayTracker.Core.Common;
using Logistic.DaysOfStayTracker.Core.Extension;

namespace Logistic.DaysOfStayTracker.Core.DayOfStays.Commands;

public record DayOfStayCreateRequest(Guid DriverId, DateOnly EntryDate, DateOnly ExitDate, ICollection<Guid> ExcludeEntities, ICollection<DayOfStay> AdditionalDayOfStays) : IValidationRequest<DayOfStay>;

public sealed class DayOfStayCreateHandler : IValidationRequestHandler<DayOfStayCreateRequest, DayOfStay>
{
    private readonly IEnumerable<IValidator<DayOfStayValidateDetail>> _validators;

    public DayOfStayCreateHandler(IEnumerable<IValidator<DayOfStayValidateDetail>> validators)
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

        var validateDetail = new DayOfStayValidateDetail(createEntity, request.ExcludeEntities, request.AdditionalDayOfStays);
        var result = await _validators.ValidateAsync(validateDetail, cancellationToken);
        return result.Map(_ => createEntity);
    }
}