using Microsoft.EntityFrameworkCore;
using SmartCampus.Application.Common.Interfaces;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Infrastructure.Persistence.Repositories;

public sealed class AttendanceRecordRepository(AppDbContext dbContext) : IAttendanceRecordRepository
{
    public void Add(AttendanceRecord record) => dbContext.AttendanceRecords.Add(record);

    public Task<AttendanceRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.AttendanceRecords.SingleOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<AttendanceRecord?> GetByStudentAndDateAsync(Guid studentId, DateOnly attendanceDate, CancellationToken cancellationToken = default) =>
        dbContext.AttendanceRecords.SingleOrDefaultAsync(
            r => r.StudentId == studentId && r.AttendanceDate == attendanceDate,
            cancellationToken);

    public async Task<IReadOnlyList<AttendanceRecord>> ListAsync(
        Guid? studentId,
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.AttendanceRecords.AsQueryable();

        if (studentId is { } id)
        {
            query = query.Where(r => r.StudentId == id);
        }

        if (fromDate is { } from)
        {
            query = query.Where(r => r.AttendanceDate >= from);
        }

        if (toDate is { } to)
        {
            query = query.Where(r => r.AttendanceDate <= to);
        }

        return await query
            .OrderByDescending(r => r.AttendanceDate)
            .ToListAsync(cancellationToken);
    }
}
