

using Microsoft.Data.Sqlite;
using Unflow.WebApi.Example.Database;
// ReSharper disable once CheckNamespace
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUnflowWebApiDatabase(
        this IServiceCollection services,
        string connectionString,
        Action<SqliteDbContextOptionsBuilder> sqliteOptionsAction = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(connectionString);

        services.AddDbContext<UnflowDatabaseContext>(options => options
            .UseUnflowWebApiDatabase(connectionString, sqliteOptionsAction)
        ).AddDbContextOperations<UnflowDatabaseContext>();

        return services;
    }

    public static DbContextOptionsBuilder UseUnflowWebApiDatabase(
        this DbContextOptionsBuilder options,
        string connectionString,
        Action<SqliteDbContextOptionsBuilder> sqliteOptionsAction = null
    )
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(connectionString);

#warning Replace nuget Microsoft.EntityFrameworkCore.Sqlite with the intended database provider

        var connectionStringBuilder = new SqliteConnectionStringBuilder(connectionString);

        var sqliteFile = new FileInfo(connectionStringBuilder.DataSource);
        if (!sqliteFile.Exists)
            sqliteFile.Directory?.Create();

        return options.UseSqlite(connectionStringBuilder.ConnectionString, sqliteOptionsAction);
    }
}