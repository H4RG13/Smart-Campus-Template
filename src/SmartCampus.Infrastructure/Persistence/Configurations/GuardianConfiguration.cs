using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Infrastructure.Persistence.Configurations;

public sealed class GuardianConfiguration : IEntityTypeConfiguration<Guardian>
{
    public void Configure(EntityTypeBuilder<Guardian> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(g => g.LastName).IsRequired().HasMaxLength(100);
        builder.Property(g => g.Phone).IsRequired().HasMaxLength(30);
        builder.Property(g => g.Email).IsRequired().HasMaxLength(256);

        builder.HasMany(g => g.StudentLinks)
            .WithOne()
            .HasForeignKey(l => l.GuardianId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(g => g.StudentLinks).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
