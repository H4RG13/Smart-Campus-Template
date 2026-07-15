using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Infrastructure.Persistence.Configurations;

public sealed class CalendarExceptionConfiguration : IEntityTypeConfiguration<CalendarException>
{
    public void Configure(EntityTypeBuilder<CalendarException> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Date).IsRequired();
        builder.Property(e => e.Type).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.Description).HasMaxLength(250);
    }
}
