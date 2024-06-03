using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Unflow.WebApi.Example.Database.Migrator;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<UnflowDatabaseContext>
{
    public UnflowDatabaseContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<UnflowDatabaseContext>();
        optionsBuilder.UseUnflowWebApiDatabase(
            configuration.GetConnectionString("UnflowDotnetWebApi"),
            o => o.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.FullName)
        );

        return new UnflowDatabaseContext(optionsBuilder.Options);
    }
}