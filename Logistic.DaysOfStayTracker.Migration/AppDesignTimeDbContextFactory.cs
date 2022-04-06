using Logistic.DaysOfStayTracker.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Logistic.DaysOfStayTracker.Migration;

public class AppDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder
            .UseSqlite("Data Source=app.db", b =>
            {
                b.MigrationsAssembly(typeof(AppDesignTimeDbContextFactory).Assembly.FullName);
            });
        
        return new AppDbContext(optionsBuilder.Options);
    }
}