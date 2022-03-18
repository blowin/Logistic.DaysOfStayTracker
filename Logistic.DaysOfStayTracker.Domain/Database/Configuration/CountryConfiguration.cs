using Logistic.DaysOfStayTracker.Core.Countries;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistic.DaysOfStayTracker.Core.Database.Configuration;

public class CountryConfiguration : EntityConfiguration<Country>
{
    protected override void ConfigureCore(EntityTypeBuilder<Country> builder)
    {
        // TODO добавить страницу для редактирования стран
        builder.Property(e => e.Name).HasMaxLength(64).IsRequired();
        
        builder.Property(e => e.IsEuropeanUnion).IsRequired();

        builder.HasData(new[]
        {
            new Country {Id = Guid.NewGuid(), Name = "Беларусь", IsEuropeanUnion = false},
            new Country {Id = Guid.NewGuid(), Name = "Россия", IsEuropeanUnion = false},
            new Country {Id = Guid.NewGuid(), Name = "Казахстан", IsEuropeanUnion = false},
            new Country {Id = Guid.NewGuid(), Name = "Киргизия", IsEuropeanUnion = false},
            new Country {Id = Guid.NewGuid(), Name = "Азербайджан", IsEuropeanUnion = false},
            new Country {Id = Guid.NewGuid(), Name = "Армения", IsEuropeanUnion = false},
            new Country {Id = Guid.NewGuid(), Name = "Грузия", IsEuropeanUnion = false},

            new Country {Id = Guid.NewGuid(), Name = "Дания", IsEuropeanUnion = true},
            new Country {Id = Guid.NewGuid(), Name = "Ирландия", IsEuropeanUnion = true},
            new Country {Id = Guid.NewGuid(), Name = "Румыния", IsEuropeanUnion = true},
            new Country {Id = Guid.NewGuid(), Name = "Словакия", IsEuropeanUnion = true},
            new Country {Id = Guid.NewGuid(), Name = "Бельгия", IsEuropeanUnion = true},
            new Country {Id = Guid.NewGuid(), Name = "Польша", IsEuropeanUnion = true},
            new Country {Id = Guid.NewGuid(), Name = "Германия", IsEuropeanUnion = true},
            new Country {Id = Guid.NewGuid(), Name = "Франция", IsEuropeanUnion = true},
            new Country {Id = Guid.NewGuid(), Name = "Италия", IsEuropeanUnion = true},
            new Country {Id = Guid.NewGuid(), Name = "Чехия", IsEuropeanUnion = true},
            new Country {Id = Guid.NewGuid(), Name = "Эстония", IsEuropeanUnion = true},
            new Country {Id = Guid.NewGuid(), Name = "Люксембург", IsEuropeanUnion = true},
            new Country {Id = Guid.NewGuid(), Name = "Швеция", IsEuropeanUnion = true},
            new Country {Id = Guid.NewGuid(), Name = "Литва", IsEuropeanUnion = true},
            new Country {Id = Guid.NewGuid(), Name = "Латвия", IsEuropeanUnion = true},
            new Country {Id = Guid.NewGuid(), Name = "Нидерланды", IsEuropeanUnion = true},
        });
    }
}