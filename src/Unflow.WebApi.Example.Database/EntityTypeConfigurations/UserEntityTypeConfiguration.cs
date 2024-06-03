using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unflow.WebApi.Example.Database.Entities;

namespace Unflow.WebApi.Example.Database.EntityTypeConfigurations;

public class UserEntityTypeConfiguration
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(e => e.Name)
            .HasMaxLength(254)
            .IsRequired();
        
        builder.Property(e => e.Id)
            .HasMaxLength(254)
            .IsRequired();
    }
}