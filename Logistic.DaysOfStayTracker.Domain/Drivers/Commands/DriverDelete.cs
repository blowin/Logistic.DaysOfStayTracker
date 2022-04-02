using CSharpFunctionalExtensions;
using Logistic.DaysOfStayTracker.Core.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistic.DaysOfStayTracker.Core.Drivers.Commands;

public record DriverDeleteRequest(Guid Id) : IValidationRequest<Unit>;

public sealed class DriverDeleteHandler : IValidationRequestHandler<DriverDeleteRequest, Unit>
{
    private readonly AppDbContext _db;
    private readonly ILogger<DriverDeleteHandler> _logger;

    public DriverDeleteHandler(AppDbContext db, ILogger<DriverDeleteHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Result<Unit, ICollection<string>>> Handle(DriverDeleteRequest request, CancellationToken cancellationToken)
    {
        var entity = await _db.Drivers.FindAsync(request.Id);
        if (entity == null)
        {
            return Result.Failure<Unit, ICollection<string>>(new List<string>
            {
                "Не найден водитель"
            });
        }

        var tran = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var dayOfStays = await _db.DayOfStays.AsTracking()
                .Where(e => e.DriverId == request.Id)
                .ToListAsync(cancellationToken: cancellationToken);
            
            _db.DayOfStays.RemoveRange(dayOfStays);
            _db.Drivers.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
            await tran.CommitAsync(cancellationToken);
            
            return Unit.Value;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{@Request}", request);
            await tran.RollbackAsync(cancellationToken);
            
            return Result.Failure<Unit, ICollection<string>>(new List<string>
            {
                e.Message
            });
        }
    }
}