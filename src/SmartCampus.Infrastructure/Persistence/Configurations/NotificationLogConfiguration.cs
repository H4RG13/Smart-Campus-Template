using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Infrastructure.Persistence.Configurations;

public sealed class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Channel).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(n => n.TriggerReason).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(n => n.Status).IsRequired().HasConversion<string>().HasMaxLength(20);

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(n => n.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Guardian>()
            .WithMany()
            .HasForeignKey(n => n.GuardianId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
