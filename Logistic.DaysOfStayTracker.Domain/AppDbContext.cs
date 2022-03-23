using Logistic.DaysOfStayTracker.Core.Countries;
using Logistic.DaysOfStayTracker.Core.DayOfStays;
using Logistic.DaysOfStayTracker.Core.Drivers;
using Microsoft.EntityFrameworkCore;

namespace Logistic.DaysOfStayTracker.Core;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public DbSet<Driver> Drivers { get; private set; } = null!;
    public DbSet<DayOfStay> DayOfStays { get; private set; } = null!;
    public DbSet<Country> Countries { get; private set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}