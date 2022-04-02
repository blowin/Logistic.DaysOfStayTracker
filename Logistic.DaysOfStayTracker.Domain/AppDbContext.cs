using Bogus;
using Logistic.DaysOfStayTracker.Core.Common;
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
    
    public async Task Initialize()
    {
        var driverFaker = new Faker<Driver>("ru").UseSeed(8080)
            .CustomInstantiator(_ => new Driver())
            .RuleFor(e => e.Id, e => e.Random.Guid())
            .RuleFor(e => e.FirstName, e => e.Person.FirstName)
            .RuleFor(e => e.LastName, e => e.Person.LastName);

        const int driverCount = 100;

        var countries = await Countries.Select(e => e.Id).ToListAsync();
            
        var drivers = driverFaker.GenerateForever().Take(driverCount).ToList();
        await Drivers.AddRangeAsync(drivers);

        var dayOfStaysFaker = new Faker<DayOfStay>("ru").UseSeed(8080)
            .CustomInstantiator(_ => new DayOfStay())
            .RuleFor(e => e.Id, e => e.Random.Guid())
            .RuleFor(e => e.DriverId, e => e.PickRandom(drivers).Id)
            .Rules((faker, stay) =>
            {
                var year = faker.Date.PastOffset();
                stay.EntryDate = DateOnly.FromDateTime(year.Date);
                stay.ExitDate = faker.Date.BetweenDateOnly(stay.EntryDate, stay.EntryDate.AddYears(1));

                stay.EntryCountryId = faker.PickRandom(countries);
                stay.ExitCountryId = faker.PickRandom(countries.Where(e => e != stay.EntryCountryId));
            });

        var f = new Faker();
        Randomizer.Seed = new Random(8080);
        foreach (var driver in drivers)
        {
            var dayOfStays = dayOfStaysFaker.GenerateForever().Take(f.Random.Int(12, 60)).ToList();
            foreach (var dayOfStay in dayOfStays)
                dayOfStay.DriverId = driver.Id;
            await DayOfStays.AddRangeAsync(dayOfStays);   
        }

        await SaveChangesAsync();
    }
}