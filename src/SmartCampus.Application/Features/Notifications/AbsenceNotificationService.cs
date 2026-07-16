using Microsoft.Extensions.Options;
using SmartCampus.Application.Common.Interfaces;
using SmartCampus.Configuration;
using SmartCampus.Domain.Entities;
using SmartCampus.Domain.Enums;

namespace SmartCampus.Application.Features.Notifications;

/// <summary>
/// Called by the Attendance feature (Phase 3/4) whenever a record's status is Absent —
/// kept as a distinct service rather than folded into RecordAttendanceUseCase so
/// Attendance doesn't grow email/guardian concerns, and so both the manual and
/// device-originated paths trigger the same notification logic exactly once.
/// </summary>
public sealed class AbsenceNotificationService(
    IGuardianRepository guardianRepository,
    INotificationLogRepository notificationLogRepository,
    IEmailSender emailSender,
    IUnitOfWork unitOfWork,
    IOptions<NotificationSettings> notificationSettings,
    IOptions<BrandingSettings> brandingSettings)
{
    public async Task NotifyAsync(Guid studentId, string studentDisplayName, DateOnly attendanceDate, CancellationToken cancellationToken = default)
    {
        if (!notificationSettings.Value.NotifyGuardiansOnAbsence)
        {
            return;
        }

        var guardians = await guardianRepository.ListByStudentAsync(studentId, cancellationToken);
        if (guardians.Count == 0)
        {
            return;
        }

        foreach (var guardian in guardians)
        {
            var status = NotificationStatus.Sent;
            try
            {
                await emailSender.SendAsync(
                    guardian.Email,
                    $"{brandingSettings.Value.SchoolName}: Absence notice for {studentDisplayName}",
                    $"{studentDisplayName} was marked absent on {attendanceDate:yyyy-MM-dd}. " +
                    "If this is unexpected, please contact the school office.",
                    cancellationToken);
            }
            catch
            {
                status = NotificationStatus.Failed;
            }

            notificationLogRepository.Add(new NotificationLog(
                studentId, guardian.Id, NotificationChannel.Email, NotificationTriggerReason.Absence, status, DateTime.UtcNow));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
