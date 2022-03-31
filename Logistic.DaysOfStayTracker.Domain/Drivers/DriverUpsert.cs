using CSharpFunctionalExtensions;
using FluentValidation;
using Logistic.DaysOfStayTracker.Core.Common;
using Logistic.DaysOfStayTracker.Core.DayOfStays;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistic.DaysOfStayTracker.Core.Drivers;

public class DriverUpsertRequest : IValidationRequest
{
    public Guid? Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public Dictionary<Guid, DayOfStay> DeletedDayOfStays { get; } = new();

    public HashSet<DayOfStay> CreateDayOfStays { get; } = new();

    public void AddDeletedDayOfStay(DayOfStay dayOfStay)
    {
        if(CreateDayOfStays.Remove(dayOfStay))
            return;

        DeletedDayOfStays.Add(dayOfStay.Id, dayOfStay);
    }
}

public record DriverUpsertModelGet(Guid Id) : IRequest<DriverUpsertRequest>;

public sealed class DriverUpsertHandler : IValidationRequestHandler<DriverUpsertRequest>, IRequestHandler<DriverUpsertModelGet, DriverUpsertRequest>
{
    private readonly AppDbContext _db;
    private readonly IEnumerable<IValidator<Driver>> _validators;
    private readonly ILogger<DriverUpsertHandler> _logger;

    public DriverUpsertHandler(AppDbContext db, IEnumerable<IValidator<Driver>> validators, ILogger<DriverUpsertHandler> logger)
    {
        _db = db;
        _validators = validators;
        _logger = logger;
    }

    public async Task<Result<Unit, ICollection<string>>> Handle(DriverUpsertRequest request, CancellationToken cancellationToken)
    {
        var driver = request.Id != null
            ? await _db.Drivers.AsTracking().FirstAsync(e => e.Id == request.Id, cancellationToken)
            : new Driver();
        
        driver.FirstName = request.FirstName ?? string.Empty;
        driver.LastName = request.LastName ?? string.Empty;
        
        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync(driver, cancellationToken);
            if (result.IsValid) 
                continue;
            
            var errors = result.Errors.Select(e => e.ErrorMessage).ToList();
            return Result.Failure<Unit, ICollection<string>>(errors);
        }

        var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            if (request.DeletedDayOfStays.Count > 0)
            {
                var removeDayOfStays = _db.DayOfStays.AsTracking()
                    .Where(e => request.DeletedDayOfStays.ContainsKey(e.Id));

                _db.DayOfStays.RemoveRange(removeDayOfStays);
            }

            if (request.CreateDayOfStays.Count > 0)
                await _db.DayOfStays.AddRangeAsync(request.CreateDayOfStays, cancellationToken);

            if (request.Id == null)
            {
                await _db.Drivers.AddAsync(driver, cancellationToken);
            }
            else
            {
                _db.Drivers.Update(driver);   
            }
        
            await _db.SaveChangesAsync(cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{@Request}", request);
            await transaction.RollbackAsync(cancellationToken);
        }
        return Unit.Value;
    }

    public async Task<DriverUpsertRequest> Handle(DriverUpsertModelGet request, CancellationToken cancellationToken)
    {
        var driver = await _db.Drivers.FirstAsync(r => r.Id == request.Id, cancellationToken);
        return new DriverUpsertRequest
        {
            Id = driver.Id,
            FirstName= driver.FirstName,
            LastName = driver.LastName
        };
    }
}