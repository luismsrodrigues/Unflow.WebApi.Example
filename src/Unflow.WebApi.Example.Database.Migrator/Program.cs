using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Extensions.Logging;
using Unflow.WebApi.Example.Database;
using Unflow.WebApi.Example.Database.Migrator;
using Unflow.WebApi.Example.Database.Migrator.Seeds;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
#pragma warning disable CA1852

using var cts = new CancellationTokenSource();

Console.CancelKeyPress += (_, eventArgs) =>
{
    // ReSharper disable once AccessToDisposedClosure
    cts.Cancel();
    eventArgs.Cancel = true;
};

using var loggerFactory = BuildNLogLoggerFactory();

ILogger<Program> logger = null;
try
{
    var builder = Host.CreateApplicationBuilder(args);

    ConfigureLogging(
        builder.Logging
    );

    ConfigureServices(
        builder.Services,
        builder.Configuration
    );

    using var host = builder.Build();

    logger = host.Services.GetRequiredService<ILogger<Program>>();

    await using var scope = host.Services.CreateAsyncScope();

    var context = scope.ServiceProvider.GetRequiredService<UnflowDatabaseContext>();

    await MigrateDatabaseAsync(
        logger,
        context,
        cts.Token
    );

    var dataSeeds = scope.ServiceProvider.GetServices<IDataSeed>();

    await SeedInitialDataAsync(
        logger,
        context,
        dataSeeds,
        "Migrations",
        DateTimeOffset.UtcNow,
        cts.Token
    );
}
catch (Exception e)
{
    if (logger == null)
    {
        await Console.Error.WriteLineAsync("Application failed with a fatal error");
        await Console.Error.WriteLineAsync(e.ToString());
    }
    else
        logger.LogCritical(e, "Application failed with a fatal error");

    throw;
}

return;

static LogFactory BuildNLogLoggerFactory()
{
    GlobalDiagnosticsContext.Set(
        "majorVersion",
        Properties.FileVersionInfo.FileMajorPart.ToString(CultureInfo.InvariantCulture)
    );
    GlobalDiagnosticsContext.Set(
        "fileVersion",
        Properties.FileVersionInfo.FileVersion
    );
    GlobalDiagnosticsContext.Set(
        "productVersion",
        Properties.FileVersionInfo.ProductVersion
    );

    return LogManager.Setup(builder =>
    {
        builder.LoadConfigurationFromFile(optional: true);
    });
}

static void ConfigureLogging(
    ILoggingBuilder logging
)
{
    logging.AddNLog();
}

static void ConfigureServices(
    IServiceCollection services,
    IConfiguration configuration
)
{
    services.AddUnflowWebApiDatabase(
        configuration.GetConnectionString("UnflowDotnetWebApi"),
        o => o.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.FullName)
    );
}

static async Task MigrateDatabaseAsync(
    ILogger logger,
    DbContext context,
    CancellationToken ct
)
{
    logger.LogDebug("Checking if there are any pending migrations to be applied");

    var pendingMigrations = (await context.Database.GetPendingMigrationsAsync(ct)).ToArray();

    if (pendingMigrations.Length == 0)
        logger.LogDebug("The are no pending migrations to be applied");
    else
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug(
                "A total of {PendingMigrationsCount} migrations will be applied to the database [PendingMigrations: {PendingMigrations}]",
                pendingMigrations.Length, string.Join(", ", pendingMigrations));
        await context.Database.MigrateAsync(ct);
    }

    logger.LogInformation("Database migrated to latest version");
}

static async Task SeedInitialDataAsync(
    ILogger logger,
    DbContext context,
    IEnumerable<IDataSeed> dataSeeds,
    string seededBy,
    DateTimeOffset seededOn,
    CancellationToken ct
)
{
    logger.LogDebug("Seeding database [By:{SeededBy} On:{SeededOn}]", seededBy, seededOn);

    await using var tx = await context.Database.BeginTransactionAsync(ct);

    foreach (var dataSeed in dataSeeds)
    {
        using (logger.BeginScope("SeedName:{SeedName}", dataSeed.GetType().Name))
        {
            logger.LogDebug("Running data seed");

            await dataSeed.SeedAsync(seededBy, seededOn, ct);

            logger.LogInformation("Data was seeded");
        }
    }

    logger.LogDebug("Saving database changes");

    await context.SaveChangesAsync(ct);
    await tx.CommitAsync(ct);

    logger.LogInformation("Database seeds were run");
}