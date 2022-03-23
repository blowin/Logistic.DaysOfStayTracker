﻿using Logistic.DaysOfStayTracker.Core.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistic.DaysOfStayTracker.Core.Countries;

public class CountryConfiguration : EntityConfiguration<Country>
{
    protected override void ConfigureCore(EntityTypeBuilder<Country> builder)
    {
        builder.Property(e => e.Name).HasMaxLength(64).IsRequired();

        builder.HasData(new[]
        {
            new Country {Id = Guid.NewGuid(), Name = "Польша" },
            new Country {Id = Guid.NewGuid(), Name = "Эстония" },
            new Country {Id = Guid.NewGuid(), Name = "Литва" },
            new Country {Id = Guid.NewGuid(), Name = "Латвия"},
        });
    }
}