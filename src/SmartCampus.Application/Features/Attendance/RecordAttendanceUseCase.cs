using Microsoft.Extensions.Options;
using SmartCampus.Application.Common.Interfaces;
using SmartCampus.Application.Features.Notifications;
using SmartCampus.Configuration;
using SmartCampus.Domain.Entities;
using SmartCampus.Domain.Enums;

namespace SmartCampus.Application.Features.Attendance;

public sealed class RecordAttendanceUseCase(
    IAttendanceRecordRepository attendanceRecordRepository,
    IStudentRepository studentRepository,
    ICurrentUserAccessor currentUserAccessor,
    IUnitOfWork unitOfWork,
    IOptions<SchoolSettings> schoolSettings,
    AbsenceNotificationService absenceNotificationService)
{
    public async Task<AttendanceRecordDto> ExecuteAsync(RecordAttendanceRequest request, CancellationToken cancellationToken = default)
    {
        var recordedByUserId = currentUserAccessor.UserId
            ?? throw new InvalidOperationException("Attendance can only be recorded by an authenticated user.");

        AttendanceStatus status;
        DateTime? checkInAtUtc = null;

        if (request.CheckInTime is { } checkInTime)
        {
            status = AttendanceStatusCalculator.Calculate(checkInTime, schoolSettings.Value);
            checkInAtUtc = DateTime.SpecifyKind(request.AttendanceDate.ToDateTime(TimeOnly.FromTimeSpan(checkInTime.ToTimeSpan())), DateTimeKind.Utc);
        }
        else
        {
            status = request.Status!.Value;
        }

        var record = new AttendanceRecord(
            request.StudentId,
            request.AttendanceDate,
            status,
            checkInAtUtc,
            recordedByUserId,
            request.Notes,
            DateTime.UtcNow);

        attendanceRecordRepository.Add(record);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (status == AttendanceStatus.Absent)
        {
            var student = await studentRepository.GetByIdAsync(request.StudentId, cancellationToken);
            if (student is not null)
            {
                await absenceNotificationService.NotifyAsync(
                    student.Id, $"{student.FirstName} {student.LastName}", request.AttendanceDate, cancellationToken);
            }
        }

        return ToDto(record);
    }

    internal static AttendanceRecordDto ToDto(AttendanceRecord record) => new()
    {
        Id = record.Id,
        StudentId = record.StudentId,
        AttendanceDate = record.AttendanceDate,
        CheckInAtUtc = record.CheckInAtUtc,
        Status = record.Status,
        RecordedByUserId = record.RecordedByUserId,
        DeviceEventId = record.DeviceEventId,
        Notes = record.Notes,
        CreatedAtUtc = record.CreatedAtUtc,
    };
}
