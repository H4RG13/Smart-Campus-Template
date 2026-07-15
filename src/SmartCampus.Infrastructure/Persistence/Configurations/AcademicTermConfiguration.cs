using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Infrastructure.Persistence.Configurations;

public sealed class AcademicTermConfiguration : IEntityTypeConfiguration<AcademicTerm>
{
    public void Configure(EntityTypeBuilder<AcademicTerm> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.Property(t => t.StartDate).IsRequired();
        builder.Property(t => t.EndDate).IsRequired();

        builder.HasMany(t => t.Exceptions)
            .WithOne()
            .HasForeignKey(e => e.AcademicTermId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(t => t.Exceptions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
