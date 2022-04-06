using Logistic.DaysOfStayTracker.Core.Common;
using Logistic.DaysOfStayTracker.Core.Drivers;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistic.DaysOfStayTracker.Core.DayOfStays;

public class DayOfStayConfiguration : EntityConfiguration<DayOfStay>
{
    protected override void ConfigureCore(EntityTypeBuilder<DayOfStay> builder)
    {
        builder.Property(e => e.EntryDate).IsRequired();
        builder.Property(e => e.ExitDate).IsRequired();
        
        builder.HasOne<Driver>().WithMany().HasForeignKey(e => e.DriverId).IsRequired();
    }
}