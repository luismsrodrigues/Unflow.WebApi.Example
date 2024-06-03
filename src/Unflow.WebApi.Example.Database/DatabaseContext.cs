using Unflow.WebApi.Example.Database.Entities;

namespace Unflow.WebApi.Example.Database;

public class UnflowDatabaseContext : DbContext
{
    public UnflowDatabaseContext(DbContextOptions<UnflowDatabaseContext> options)
        : base(options)
    {}

    public DbSet<User> User { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
