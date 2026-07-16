using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Infrastructure.Persistence.Configurations;

public sealed class DeviceEventConfiguration : IEntityTypeConfiguration<DeviceEvent>
{
    public void Configure(EntityTypeBuilder<DeviceEvent> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.DeviceEventId).IsRequired().HasMaxLength(100);
        builder.Property(e => e.RfidTagId).IsRequired().HasMaxLength(100);
        builder.Property(e => e.ProcessingStatus).IsRequired().HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(e => new { e.DeviceId, e.DeviceEventId }).IsUnique();

        builder.HasOne<Device>()
            .WithMany()
            .HasForeignKey(e => e.DeviceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
