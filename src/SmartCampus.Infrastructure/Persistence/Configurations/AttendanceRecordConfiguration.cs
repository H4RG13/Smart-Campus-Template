using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Infrastructure.Persistence.Configurations;

public sealed class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.AttendanceDate).IsRequired();
        builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(r => r.Notes).HasMaxLength(500);

        builder.HasIndex(r => new { r.StudentId, r.AttendanceDate }).IsUnique();

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(r => r.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<DeviceEvent>()
            .WithMany()
            .HasForeignKey(r => r.DeviceEventId)
            .OnDelete(DeleteBehavior.Restrict);

        // Manual entry (RecordedByUserId) and device-originated entry (DeviceEventId) are
        // mutually exclusive — see DATABASE_DESIGN.md.
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_AttendanceRecords_OriginExclusive",
            "(\"RecordedByUserId\" IS NOT NULL AND \"DeviceEventId\" IS NULL) OR " +
            "(\"RecordedByUserId\" IS NULL AND \"DeviceEventId\" IS NOT NULL)"));
    }
}
