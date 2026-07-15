using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Infrastructure.Persistence.Configurations;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(s => s.LastName).IsRequired().HasMaxLength(100);

        builder.Property(s => s.StudentNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(s => s.StudentNumber).IsUnique();

        builder.Property(s => s.RfidTagId).HasMaxLength(100);
        builder.HasIndex(s => s.RfidTagId).IsUnique().HasFilter("\"RfidTagId\" IS NOT NULL");

        builder.HasOne<SchoolClass>()
            .WithMany()
            .HasForeignKey(s => s.ClassId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
