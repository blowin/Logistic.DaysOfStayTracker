using Logistic.DaysOfStayTracker.Core.DayOfStays;
using Logistic.DaysOfStayTracker.Core.Drivers;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistic.DaysOfStayTracker.Core.Database.Configuration;

public class DayOfStayConfiguration : EntityConfiguration<DayOfStay>
{
    protected override void ConfigureCore(EntityTypeBuilder<DayOfStay> builder)
    {
        builder.Property(e => e.Start).IsRequired();
        builder.Property(e => e.End).IsRequired();
        
        builder.HasOne<Driver>().WithMany().HasForeignKey(e => e.DriverId).IsRequired();
    }
}