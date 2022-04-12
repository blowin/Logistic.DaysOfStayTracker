using System;
using System.Threading.Tasks;
using Logistic.DaysOfStayTracker.Core;
using Logistic.DaysOfStayTracker.Core.Drivers;
using Logistic.DaysOfStayTracker.Core.Drivers.Commands;
using Logistic.DaysOfStayTracker.DependencyInjection;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Logistic.DaysOfStayTracker.Test;

public class TestServiceProviderFactory
{
    public bool AddDriver { get; set; } = true;
    
    public async Task<CreateScope> CreateAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var appConfig = new AppServicesConfiguration(builder => builder.UseSqlite(connection));
        
        var provider = new ServiceCollection()
            .AddAppServices(appConfig)
            .BuildServiceProvider();
        
        using var scope = provider.CreateScope();
        
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        Driver? driver = null;
        if (AddDriver)
        {
            var request = scope.ServiceProvider.GetRequiredService<DriverUpsertRequest>()
                .WithName("Андрей", "Покров")
                .WithExpiryDate(new DateTime(2000, 1, 1));
         
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var upsertResult = await mediator.Send(request);
            driver = upsertResult.Value;
        }

        return new CreateScope(provider, connection, driver);
    }
    
    public class CreateScope : IDisposable
    {
        private SqliteConnection _connection;
        
        public Driver? Driver { get; private set; }
        
        public IServiceProvider Provider { get; }
        
        public CreateScope(IServiceProvider provider, SqliteConnection connection, Driver? driver)
        {
            Provider = provider;
            _connection = connection;
            Driver = driver;
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}