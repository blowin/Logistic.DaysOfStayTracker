using Logistic.DaysOfStayTracker.Core.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Logistic.DaysOfStayTracker.Core.Drivers;

public class DriverUpsertRequest : IRequest
{
    public Guid? Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public record DriverUpsertModelGet(Guid Id) : IRequest<DriverUpsertRequest>;

public sealed class DriverUpsertHandler : IRequestHandler<DriverUpsertRequest>, IRequestHandler<DriverUpsertModelGet, DriverUpsertRequest>
{
    private AppDbContext _db;

    public DriverUpsertHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Unit> Handle(DriverUpsertRequest request, CancellationToken cancellationToken)
    {
        // TODO validate
        var driver = request.Id != null
            ? await _db.Drivers.AsTracking().FirstAsync(e => e.Id == request.Id, cancellationToken)
            : new Driver();
        
        driver.FirstName = request.FirstName ?? string.Empty;
        driver.LastName = request.LastName ?? string.Empty;
        
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