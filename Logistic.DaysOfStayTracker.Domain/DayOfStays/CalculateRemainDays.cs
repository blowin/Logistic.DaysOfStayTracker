using Logistic.DaysOfStayTracker.Core.Database;
using MediatR;

namespace Logistic.DaysOfStayTracker.Core.DayOfStays;

public record CalculateRemainDaysRequest() : IRequest<CalculateRemainDaysResponse>;

public record CalculateRemainDaysResponse(int RemainDays, DateOnly DateOfAddDays, int AddDays);

public sealed class CalculateRemainDaysHandler 
    : IRequestHandler<CalculateRemainDaysRequest, CalculateRemainDaysResponse>
{
    private AppDbContext _db;

    public CalculateRemainDaysHandler(AppDbContext db)
    {
        _db = db;
    }

    public Task<CalculateRemainDaysResponse> Handle(CalculateRemainDaysRequest request, CancellationToken cancellationToken)
    {
        // TODO: real calculation
        /*
        var calculateDays = _db.DayOfStays
            .Include(e => e.EntryCountry)
            .Include(e => e.ExitCountry)
            .Where(e => e.EntryDate >= request.FromDate && e.ExitDate <= request.ToDate)
            .Select(r => new DayOfStayDetail(r.EntryDate, r.EntryCountry!.IsEuropeanUnion,
                r.ExitDate, r.ExitCountry!.IsEuropeanUnion))
            .ToListAsync(cancellationToken: cancellationToken);
        */
        var response = new CalculateRemainDaysResponse(90, DateOnly.FromDateTime(DateTime.Today), 0);
        return Task.FromResult(response);
    }
    
    public record DayOfStayDetail(DateOnly EntryDate, bool IsEuropeanUnionEntryCountry, 
        DateOnly ExitDate, bool IsEuropeanUnionExitCountry);
}