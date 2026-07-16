using SmartCampus.Application.Common.Interfaces;
using SmartCampus.Application.Features.Notifications;
using SmartCampus.Domain.Enums;

namespace SmartCampus.Application.Features.Attendance;

public sealed class CorrectAttendanceUseCase(
    IAttendanceRecordRepository attendanceRecordRepository,
    IStudentRepository studentRepository,
    IUnitOfWork unitOfWork,
    AbsenceNotificationService absenceNotificationService)
{
    public async Task<AttendanceRecordDto?> ExecuteAsync(Guid id, CorrectAttendanceRequest request, CancellationToken cancellationToken = default)
    {
        var record = await attendanceRecordRepository.GetByIdAsync(id, cancellationToken);
        if (record is null)
        {
            return null;
        }

        var wasAbsent = record.Status == AttendanceStatus.Absent;
        record.Correct(request.Status, request.Notes);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (!wasAbsent && request.Status == AttendanceStatus.Absent)
        {
            var student = await studentRepository.GetByIdAsync(record.StudentId, cancellationToken);
            if (student is not null)
            {
                await absenceNotificationService.NotifyAsync(
                    student.Id, $"{student.FirstName} {student.LastName}", record.AttendanceDate, cancellationToken);
            }
        }

        return RecordAttendanceUseCase.ToDto(record);
    }
}
