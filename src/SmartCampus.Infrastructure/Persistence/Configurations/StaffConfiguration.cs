using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Infrastructure.Persistence.Configurations;

public sealed class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(s => s.LastName).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Position).IsRequired().HasMaxLength(100);

        builder.Property(s => s.EmployeeNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(s => s.EmployeeNumber).IsUnique();
    }
}
