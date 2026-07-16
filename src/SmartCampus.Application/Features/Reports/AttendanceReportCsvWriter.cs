using System.Text;

namespace SmartCampus.Application.Features.Reports;

/// <summary>Manual CSV building — the data is flat and small enough that a CSV library
/// would be an unneeded dependency (RULES.md #8).</summary>
public static class AttendanceReportCsvWriter
{
    public static string Write(IReadOnlyList<AttendanceReportRowDto> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Student Number,Student Name,Total Days,On Time,Late,Absent,Excused Absence,Attendance Rate %");

        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(',',
                Escape(row.StudentNumber),
                Escape(row.StudentName),
                row.TotalDays,
                row.OnTimeCount,
                row.LateCount,
                row.AbsentCount,
                row.ExcusedAbsenceCount,
                row.AttendanceRate));
        }

        return sb.ToString();
    }

    private static string Escape(string value) =>
        value.Contains(',') || value.Contains('"')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
}
