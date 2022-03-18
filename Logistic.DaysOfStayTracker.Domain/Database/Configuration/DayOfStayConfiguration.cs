using Logistic.DaysOfStayTracker.Core.Countries;
using Logistic.DaysOfStayTracker.Core.DayOfStays;
using Logistic.DaysOfStayTracker.Core.Drivers;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistic.DaysOfStayTracker.Core.Database.Configuration;

public class DayOfStayConfiguration : EntityConfiguration<DayOfStay>
{
    protected override void ConfigureCore(EntityTypeBuilder<DayOfStay> builder)
    {
        builder.Property(e => e.EntryDate).IsRequired();
        builder.Property(e => e.ExitDate).IsRequired();

        builder.HasOne<Country>(e => e.EntryCountry)
            .WithMany()
            .HasForeignKey(e => e.EntryCountryId)
            .IsRequired();
        
        builder.HasOne<Country>(e => e.ExitCountry)
            .WithMany()
            .HasForeignKey(e => e.ExitCountryId)
            .IsRequired();
        
        // TODO cascade action (delete)
        builder.HasOne<Driver>().WithMany().HasForeignKey(e => e.DriverId).IsRequired();
    }
}