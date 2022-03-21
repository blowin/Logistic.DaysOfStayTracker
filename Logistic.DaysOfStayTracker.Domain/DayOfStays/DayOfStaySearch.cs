using Logistic.DaysOfStayTracker.Core.Database;
using Logistic.DaysOfStayTracker.Core.Extension;
using MediatR;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace Logistic.DaysOfStayTracker.Core.DayOfStays;

public record DayOfStaySearchRequest : IRequest<IPagedList<DayOfStaySearchResponse>>
{
    public int Page { get; set; } = 1;
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
    public Guid? DriverId { get; set; }
    public DateTime? Year { get; set; }
}

public record DayOfStaySearchResponse
{
    public Guid Id { get; set; }
    public DateOnly EntryDate { get; set; }
    public DateOnly ExitDate { get; set; }

    public string EntryCountryName { get; set; } = string.Empty;
    public string ExitCountryName { get; set; } = string.Empty;
    
}

public sealed class DayOfStaySearchHandler : IRequestHandler<DayOfStaySearchRequest, IPagedList<DayOfStaySearchResponse>>
{
    private readonly AppDbContext _dbContext;

    public DayOfStaySearchHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<IPagedList<DayOfStaySearchResponse>> Handle(DayOfStaySearchRequest request, CancellationToken cancellationToken)
    {
        IQueryable<DayOfStay> query = _dbContext.DayOfStays
                .Include(e => e.EntryCountry)
                .Include(e => e.ExitCountry)
            .OrderByDescending(e => e.EntryDate)
            .ThenByDescending(e => e.ExitDate);

        if (request.Start.HasValue)
        {
            var start = DateOnly.FromDateTime(request.Start.Value);
            query = query.Where(e => e.EntryDate <= start && e.ExitDate >= start);
        }
        
        if (request.End.HasValue)
        {
            var end = DateOnly.FromDateTime(request.End.Value);
            query = query.Where(e => e.EntryDate <= end && e.ExitDate >= end);
        }

        if (request.Year.HasValue)
        {
            var year = request.Year.Value.Year;
            query = query.Where(e => e.EntryDate.Year == year || e.ExitDate.Year == year);
        }

        if (request.DriverId.HasValue)
        {
            var driverId = request.DriverId.Value;
            query = query.Where(e => e.DriverId == driverId);
        }

        return query
            .Select(dos => new DayOfStaySearchResponse
            {
                Id = dos.Id,
                EntryDate = dos.EntryDate,
                ExitDate = dos.ExitDate,
                EntryCountryName = dos.EntryCountry!.Name,
                ExitCountryName = dos.ExitCountry!.Name,
            })
            .ToPagedListAsync(request.Page, cancellationToken);
    }
}