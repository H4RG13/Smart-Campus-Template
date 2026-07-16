using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Infrastructure.Persistence.Configurations;

public sealed class DeviceHeartbeatConfiguration : IEntityTypeConfiguration<DeviceHeartbeat>
{
    public void Configure(EntityTypeBuilder<DeviceHeartbeat> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.FirmwareVersion).IsRequired().HasMaxLength(50);

        builder.HasIndex(h => new { h.DeviceId, h.ReceivedAtUtc });

        builder.HasOne<Device>()
            .WithMany()
            .HasForeignKey(h => h.DeviceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
