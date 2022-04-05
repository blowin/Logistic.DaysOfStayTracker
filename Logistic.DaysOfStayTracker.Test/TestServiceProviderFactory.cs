using System;
using System.Threading.Tasks;
using Logistic.DaysOfStayTracker.Core;
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

        if (AddDriver)
        {
            await scope.ServiceProvider.GetRequiredService<IMediator>().Send(new DriverUpsertRequest
            {
                FirstName = "Андрей",
                LastName = "Покров"
            });   
        }

        return new CreateScope(provider, connection);
    }
    
    public class CreateScope : IDisposable
    {
        private SqliteConnection _connection;
        
        public IServiceProvider Provider { get; }
        
        public CreateScope(IServiceProvider provider, SqliteConnection connection)
        {
            Provider = provider;
            _connection = connection;
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}