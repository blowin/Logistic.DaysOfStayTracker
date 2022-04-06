using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Logistic.DaysOfStayTracker.Core.DayOfStays.Commands;

public record DayOfStaySearchRequest(DateTime Start, Guid DriverId) : IRequest<List<DayOfStay>>;

public sealed class DayOfStaySearchHandler : IRequestHandler<DayOfStaySearchRequest, List<DayOfStay>>
{
    private readonly AppDbContext _dbContext;

    public DayOfStaySearchHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<DayOfStay>> Handle(DayOfStaySearchRequest request, CancellationToken cancellationToken)
    {
        var start = DateOnly.FromDateTime(request.Start);
        return _dbContext.DayOfStays
            .OrderByDescending(e => e.EntryDate)
            .ThenByDescending(e => e.ExitDate)
            .Where(e => e.EntryDate >= start && e.DriverId == request.DriverId)
            .ToListAsync(cancellationToken);
    }
}