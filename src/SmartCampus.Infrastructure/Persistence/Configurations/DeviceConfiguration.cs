using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Infrastructure.Persistence.Configurations;

public sealed class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.DeviceName).IsRequired().HasMaxLength(100);
        builder.Property(d => d.AuthTokenHash).IsRequired();
        builder.Property(d => d.FirmwareVersion).HasMaxLength(50);
        builder.Property(d => d.Location).HasMaxLength(200);

        builder.Property(d => d.HardwareId).IsRequired().HasMaxLength(100);
        builder.HasIndex(d => d.HardwareId).IsUnique();
    }
}
