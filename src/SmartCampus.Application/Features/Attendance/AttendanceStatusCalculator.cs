using SmartCampus.Configuration;
using SmartCampus.Domain.Enums;

namespace SmartCampus.Application.Features.Attendance;

/// <summary>
/// The lateness rule lives here (Application), never in firmware or the API layer —
/// see RULES.md #4. A device-originated check-in (Phase 4) will call this same
/// calculator so the rule is defined exactly once.
/// </summary>
public static class AttendanceStatusCalculator
{
    public static AttendanceStatus Calculate(TimeOnly checkInTime, SchoolSettings schoolSettings)
    {
        var lateCutoff = schoolSettings.SchoolDayStartTime.AddMinutes(schoolSettings.AttendanceLateThresholdMinutes);
        return checkInTime <= lateCutoff ? AttendanceStatus.OnTime : AttendanceStatus.Late;
    }
}
