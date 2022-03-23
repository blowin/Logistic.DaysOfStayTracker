﻿using CSharpFunctionalExtensions;
using FluentValidation;
using Logistic.DaysOfStayTracker.Core.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Logistic.DaysOfStayTracker.Core.Drivers;

public class DriverUpsertRequest : IValidationRequest
{
    public Guid? Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public record DriverUpsertModelGet(Guid Id) : IRequest<DriverUpsertRequest>;

public sealed class DriverUpsertHandler : IValidationRequestHandler<DriverUpsertRequest>, IRequestHandler<DriverUpsertModelGet, DriverUpsertRequest>
{
    private AppDbContext _db;
    private IEnumerable<IValidator<Driver>> _validators;

    public DriverUpsertHandler(AppDbContext db, IEnumerable<IValidator<Driver>> validators)
    {
        _db = db;
        _validators = validators;
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
        
        if (request.Id == null)
        {
            await _db.Drivers.AddAsync(driver, cancellationToken);
        }
        else
        {
            _db.Drivers.Update(driver);   
        }
        
        await _db.SaveChangesAsync(cancellationToken);
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