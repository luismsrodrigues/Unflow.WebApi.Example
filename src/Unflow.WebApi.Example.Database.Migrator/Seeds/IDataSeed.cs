namespace Unflow.WebApi.Example.Database.Migrator.Seeds;

public interface IDataSeed
{
    Task SeedAsync(string seededBy, DateTimeOffset seededOn, CancellationToken ct);
}