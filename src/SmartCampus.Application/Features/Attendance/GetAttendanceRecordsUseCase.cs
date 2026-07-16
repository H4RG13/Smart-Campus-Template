using SmartCampus.Application.Common.Interfaces;

namespace SmartCampus.Application.Features.Attendance;

public sealed class GetAttendanceRecordsUseCase(IAttendanceRecordRepository attendanceRecordRepository)
{
    public async Task<IReadOnlyList<AttendanceRecordDto>> ExecuteAsync(
        Guid? studentId,
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken = default)
    {
        var records = await attendanceRecordRepository.ListAsync(studentId, fromDate, toDate, cancellationToken);
        return records.Select(RecordAttendanceUseCase.ToDto).ToList();
    }
}
