using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PasswordHash).IsRequired();

        builder.HasMany(u => u.Roles)
            .WithMany()
            .UsingEntity(join => join.ToTable("UserRoles"));

        builder.Navigation(u => u.Roles).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
