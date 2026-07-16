using SmartCampus.Domain.Entities;

namespace SmartCampus.Application.Common.Interfaces;

public interface IAttendanceRecordRepository
{
    void Add(AttendanceRecord record);
    Task<AttendanceRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AttendanceRecord?> GetByStudentAndDateAsync(Guid studentId, DateOnly attendanceDate, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AttendanceRecord>> ListAsync(
        Guid? studentId,
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken = default);
}
