using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Infrastructure.Persistence.Configurations;

public sealed class SchoolClassConfiguration : IEntityTypeConfiguration<SchoolClass>
{
    public void Configure(EntityTypeBuilder<SchoolClass> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);

        builder.HasOne<AcademicTerm>()
            .WithMany()
            .HasForeignKey(c => c.AcademicTermId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
