using FluentValidation;
using Logistic.DaysOfStayTracker.Core;
using Logistic.DaysOfStayTracker.Core.DayOfStays;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Logistic.DaysOfStayTracker.DependencyInjection;

public static class DependencyInjectionExt
{
    public static IServiceCollection AddAppServices(this IServiceCollection self,
        Action<DbContextOptionsBuilder> dbConfiguration)
    {
        return self
            .AddDbContextPool<AppDbContext>(dbConfiguration)
            .AddValidatorsFromAssembly(typeof(DayOfStaySearchRequest).Assembly)
            .AddMediatR(typeof(DayOfStaySearchRequest).Assembly)
            .AddLogging()
            .AddScoped(typeof(Repository<>));
    }
}