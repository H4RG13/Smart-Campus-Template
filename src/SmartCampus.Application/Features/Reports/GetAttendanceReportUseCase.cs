using SmartCampus.Application.Common.Interfaces;
using SmartCampus.Domain.Enums;

namespace SmartCampus.Application.Features.Reports;

public sealed class GetAttendanceReportUseCase(
    IAttendanceRecordRepository attendanceRecordRepository,
    IStudentRepository studentRepository)
{
    public async Task<IReadOnlyList<AttendanceReportRowDto>> ExecuteAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var records = await attendanceRecordRepository.ListAsync(studentId: null, fromDate, toDate, cancellationToken);
        var students = await studentRepository.ListAsync(cancellationToken);
        var studentsById = students.ToDictionary(s => s.Id);

        return records
            .GroupBy(r => r.StudentId)
            .Where(g => studentsById.ContainsKey(g.Key))
            .Select(g =>
            {
                var student = studentsById[g.Key];
                var onTime = g.Count(r => r.Status == AttendanceStatus.OnTime);
                var late = g.Count(r => r.Status == AttendanceStatus.Late);
                var absent = g.Count(r => r.Status == AttendanceStatus.Absent);
                var excused = g.Count(r => r.Status == AttendanceStatus.ExcusedAbsence);
                var total = onTime + late + absent + excused;

                return new AttendanceReportRowDto
                {
                    StudentId = student.Id,
                    StudentName = $"{student.FirstName} {student.LastName}",
                    StudentNumber = student.StudentNumber,
                    TotalDays = total,
                    OnTimeCount = onTime,
                    LateCount = late,
                    AbsentCount = absent,
                    ExcusedAbsenceCount = excused,
                    AttendanceRate = total == 0 ? 0 : Math.Round((onTime + late) * 100.0 / total, 1),
                };
            })
            .OrderBy(r => r.StudentName)
            .ToList();
    }
}
