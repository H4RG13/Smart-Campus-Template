using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Infrastructure.Persistence.Configurations;

public sealed class GuardianStudentConfiguration : IEntityTypeConfiguration<GuardianStudent>
{
    public void Configure(EntityTypeBuilder<GuardianStudent> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Relationship).IsRequired().HasMaxLength(50);

        builder.HasIndex(l => new { l.GuardianId, l.StudentId }).IsUnique();

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(l => l.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
