using Logistic.DaysOfStayTracker.Core.Drivers;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistic.DaysOfStayTracker.Core.Database.Configuration;

public class DriverConfiguration : EntityConfiguration<Driver>
{
    protected override void ConfigureCore(EntityTypeBuilder<Driver> builder)
    {
        builder.Property(e => e.FirstName).HasMaxLength(32).IsRequired();
        builder.Property(e => e.LastName).HasMaxLength(32).IsRequired();
    }
}