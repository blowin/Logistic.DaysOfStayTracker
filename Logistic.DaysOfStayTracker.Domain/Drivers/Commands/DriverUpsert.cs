using CSharpFunctionalExtensions;
using FluentValidation;
using Logistic.DaysOfStayTracker.Core.Common;
using Logistic.DaysOfStayTracker.Core.DayOfStays;
using Logistic.DaysOfStayTracker.Core.Extension;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistic.DaysOfStayTracker.Core.Drivers.Commands;

public class DriverUpsertRequest : IValidationRequest<Driver>
{
    public Guid? Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public Dictionary<Guid, DayOfStay> DeletedDayOfStays { get; } = new();

    public HashSet<DayOfStay> CreateDayOfStays { get; } = new();

    public bool HasDeleteOrCreateDateOfStays => DeletedDayOfStays.Count > 0 || CreateDayOfStays.Count > 0;
    
    public void AddDeletedDayOfStay(DayOfStay dayOfStay)
    {
        if(CreateDayOfStays.Remove(dayOfStay))
            return;

        DeletedDayOfStays.Add(dayOfStay.Id, dayOfStay);
    }
}

public record DriverUpsertModelGet(Guid Id) : IRequest<DriverUpsertRequest>;

public sealed class DriverUpsertHandler : IValidationRequestHandler<DriverUpsertRequest, Driver>, IRequestHandler<DriverUpsertModelGet, DriverUpsertRequest>
{
    private readonly AppDbContext _db;
    private readonly IEnumerable<IValidator<Driver>> _driverValidators;
    private readonly IEnumerable<IValidator<DayOfStay>> _dayOfStayValidators;
    private readonly ILogger<DriverUpsertHandler> _logger;

    public DriverUpsertHandler(AppDbContext db, IEnumerable<IValidator<Driver>> driverValidators, IEnumerable<IValidator<DayOfStay>> dayOfStayValidators, 
        ILogger<DriverUpsertHandler> logger)
    {
        _db = db;
        _driverValidators = driverValidators;
        _logger = logger;
        _dayOfStayValidators = dayOfStayValidators;
    }

    public async Task<Result<Driver, ICollection<string>>> Handle(DriverUpsertRequest request, CancellationToken cancellationToken)
    {
        var driver = request.Id != null
            ? await _db.Drivers.AsTracking().FirstAsync(e => e.Id == request.Id, cancellationToken)
            : new Driver();
        
        if(!string.IsNullOrEmpty(request.FirstName))
            driver.FirstName = request.FirstName;
        
        if(!string.IsNullOrEmpty(request.LastName))
            driver.LastName = request.LastName;

        var result = await _driverValidators.ValidateAsync(driver, cancellationToken);
        if (result.IsFailure)
            return Result.Failure<Driver, ICollection<string>>(result.Error);

        var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            if (request.Id == null)
            {
                await _db.Drivers.AddAsync(driver, cancellationToken);
                request.Id = driver.Id;
                foreach (var requestCreateDayOfStay in request.CreateDayOfStays)
                    requestCreateDayOfStay.DriverId = driver.Id;
            }
            else
            {
                _db.Drivers.Update(driver);   
            }
            
            if (request.DeletedDayOfStays.Count > 0)
            {
                var removeKeys = request.DeletedDayOfStays.Keys.ToList();
                var removeDayOfStays = _db.DayOfStays.AsTracking()
                    .Where(e => removeKeys.Contains(e.Id))
                    .AsEnumerable();

                _db.DayOfStays.RemoveRange(removeDayOfStays);
            }

            if (request.CreateDayOfStays.Count > 0)
            {
                await _db.DayOfStays.AddRangeAsync(request.CreateDayOfStays, cancellationToken);

                foreach (var createDayOfStay in request.CreateDayOfStays)
                {
                    var dayOfStayValidateResult = await _dayOfStayValidators.ValidateAsync(createDayOfStay, cancellationToken);
                    if (dayOfStayValidateResult.IsSuccess)
                        continue;
                    
                    await transaction.RollbackAsync(cancellationToken);
                    return Result.Failure<Driver, ICollection<string>>(dayOfStayValidateResult.Error);
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{@Request}", request);
            await transaction.RollbackAsync(cancellationToken);
        }
        return driver;
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