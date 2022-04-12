using CSharpFunctionalExtensions;
using FluentValidation;
using Logistic.DaysOfStayTracker.Core.Common;
using Logistic.DaysOfStayTracker.Core.DayOfStays;
using Logistic.DaysOfStayTracker.Core.Extension;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Logistic.DaysOfStayTracker.Core.Drivers.Commands;

public class DriverUpsertRequest : IValidationRequest<Driver>
{
    private readonly IEnumerable<IValidator<DayOfStayValidateDetail>> _dayOfStayValidators;

    public DriverUpsertRequest(IEnumerable<IValidator<DayOfStayValidateDetail>> dayOfStayValidators)
    {
        _dayOfStayValidators = dayOfStayValidators;
    }

    public Guid? Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public UpdateProperty<DateTime?> VisaExpiryDate { get; set; } = UpdateProperty.NonChanged<DateTime?>();

    public Dictionary<Guid, DayOfStay> DeletedDayOfStays { get; } = new();

    public HashSet<DayOfStay> CreateDayOfStays { get; } = new();

    public bool HasDeleteOrCreateDateOfStays => DeletedDayOfStays.Count > 0 || CreateDayOfStays.Count > 0;

    public DriverUpsertRequest For(Driver driver)
    {
        Id = driver.Id;
        FirstName = driver.FirstName;
        LastName = driver.LastName;
        VisaExpiryDate = UpdateProperty.Changed(driver.VisaExpiryDate?.AsDateTime());
        return this;
    }

    public DriverUpsertRequest WithId(Guid id)
    {
        Id = id;
        return this;
    }
    
    public DriverUpsertRequest WithName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
        return this;
    }
    
    public DriverUpsertRequest WithExpiryDate(DateOnly? expiryDate)
    {
        return WithExpiryDate(expiryDate?.AsDateTime());
    }
    
    public DriverUpsertRequest WithExpiryDate(DateTime? expiryDate)
    {
        VisaExpiryDate = UpdateProperty.Changed(expiryDate);
        return this;
    }
    
    public void AddDeletedDayOfStay(DayOfStay dayOfStay)
    {
        if(CreateDayOfStays.Remove(dayOfStay))
            return;

        DeletedDayOfStays.Add(dayOfStay.Id, dayOfStay);
    }

    public async Task<Result<DayOfStay, ICollection<string>>> AddCreateDayOfStayAsync(Guid driverId, DateOnly entryDate, DateOnly exitDate,
        CancellationToken cancellationToken = default)
    {
        var createEntity = new DayOfStay
        {
            DriverId = driverId,
            EntryDate = entryDate,
            ExitDate = exitDate
        };

        var validateDetail = new DayOfStayValidateDetail(createEntity, DeletedDayOfStays.Keys, CreateDayOfStays);
        var result = await _dayOfStayValidators.ValidateAsync(validateDetail, cancellationToken);
        
        if (result.IsSuccess)
            CreateDayOfStays.Add(createEntity);
        
        return result.Map(_ => createEntity);
    }
}

public record DriverUpsertModelGet(Guid Id) : IRequest<DriverUpsertRequest>;

public sealed class DriverUpsertHandler : IValidationRequestHandler<DriverUpsertRequest, Driver>, IRequestHandler<DriverUpsertModelGet, DriverUpsertRequest>
{
    private readonly AppDbContext _db;
    private readonly IEnumerable<IValidator<Driver>> _driverValidators;
    private readonly ILogger<DriverUpsertHandler> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DriverUpsertHandler(AppDbContext db, IEnumerable<IValidator<Driver>> driverValidators, 
        ILogger<DriverUpsertHandler> logger, IServiceProvider serviceProvider)
    {
        _db = db;
        _driverValidators = driverValidators;
        _logger = logger;
        _serviceProvider = serviceProvider;
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

        if (request.VisaExpiryDate.IsChanged)
        {
            var date = request.VisaExpiryDate.Value;
            driver.VisaExpiryDate = date == null ? null : DateOnly.FromDateTime(date.Value);   
        }

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
                var removeKeys = request.DeletedDayOfStays.Keys;
                var removeDayOfStays = _db.DayOfStays.AsTracking()
                    .Where(e => removeKeys.Contains(e.Id))
                    .AsEnumerable();

                _db.DayOfStays.RemoveRange(removeDayOfStays);
            }

            if (request.CreateDayOfStays.Count > 0)
                await _db.DayOfStays.AddRangeAsync(request.CreateDayOfStays, cancellationToken);

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
        return _serviceProvider.GetRequiredService<DriverUpsertRequest>().For(driver);
    }
}