using FluentValidation;
using Logistic.DaysOfStayTracker.Core;
using Logistic.DaysOfStayTracker.Core.DayOfStays.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Logistic.DaysOfStayTracker.DependencyInjection;

public sealed class AppServicesConfiguration
{
    public AppServicesConfiguration(Action<DbContextOptionsBuilder> dbConfiguration)
    {
        DbConfiguration = dbConfiguration;
    }

    public Action<DbContextOptionsBuilder> DbConfiguration { get; }
    public Action<ILoggingBuilder>? LoggingConfiguration { get; set; }
}

public static class DependencyInjectionExt
{
    public static IServiceCollection AddAppServices(this IServiceCollection self, AppServicesConfiguration configuration)
    {
        self
            .AddDbContextPool<AppDbContext>(configuration.DbConfiguration)
            .AddValidatorsFromAssembly(typeof(DayOfStaySearchRequest).Assembly)
            .AddMediatR(typeof(DayOfStaySearchRequest).Assembly);

        if (configuration.LoggingConfiguration == null)
        {
            self.AddLogging();
        }
        else
        {
            self.AddLogging(configuration.LoggingConfiguration);
        }

        return self;
    }
}