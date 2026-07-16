using SmartCampus.Application.Common.Interfaces;
using SmartCampus.Domain.Enums;

namespace SmartCampus.Application.Features.Attendance;

public sealed class GetAttendanceSummaryUseCase(IAttendanceRecordRepository attendanceRecordRepository)
{
    public async Task<IReadOnlyList<AttendanceDaySummaryDto>> ExecuteAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var records = await attendanceRecordRepository.ListAsync(studentId: null, fromDate, toDate, cancellationToken);

        return records
            .GroupBy(r => r.AttendanceDate)
            .OrderBy(g => g.Key)
            .Select(g => new AttendanceDaySummaryDto
            {
                Date = g.Key,
                OnTimeCount = g.Count(r => r.Status == AttendanceStatus.OnTime),
                LateCount = g.Count(r => r.Status == AttendanceStatus.Late),
                AbsentCount = g.Count(r => r.Status == AttendanceStatus.Absent),
                ExcusedAbsenceCount = g.Count(r => r.Status == AttendanceStatus.ExcusedAbsence),
            })
            .ToList();
    }
}
