using FluentValidation;
using SmartCampus.Application.Common.Interfaces;
using SmartCampus.Domain.Enums;

namespace SmartCampus.Application.Features.Attendance;

public sealed class RecordAttendanceRequestValidator : AbstractValidator<RecordAttendanceRequest>
{
    public RecordAttendanceRequestValidator(
        IStudentRepository studentRepository,
        IClassRepository classRepository,
        IAcademicTermRepository termRepository,
        IAttendanceRecordRepository attendanceRecordRepository)
    {
        RuleFor(x => x.StudentId)
            .MustAsync(async (id, cancellationToken) => await studentRepository.GetByIdAsync(id, cancellationToken) is not null)
            .WithMessage("Student does not exist.");

        RuleFor(x => x)
            .Must(x => x.CheckInTime is not null || x.Status is AttendanceStatus.Absent or AttendanceStatus.ExcusedAbsence)
            .WithMessage("Provide a check-in time, or an explicit status of Absent/ExcusedAbsence.")
            .WithName("CheckInTime");

        RuleFor(x => x)
            .MustAsync(async (request, cancellationToken) =>
                await attendanceRecordRepository.GetByStudentAndDateAsync(request.StudentId, request.AttendanceDate, cancellationToken) is null)
            .WithMessage("An attendance record for this student and date already exists — use the correction endpoint instead.")
            .WithName("AttendanceDate");

        RuleFor(x => x)
            .MustAsync(async (request, cancellationToken) =>
            {
                var student = await studentRepository.GetByIdAsync(request.StudentId, cancellationToken);
                if (student is null)
                {
                    return true; // already reported by the StudentId rule above
                }

                var schoolClass = await classRepository.GetByIdAsync(student.ClassId, cancellationToken);
                if (schoolClass is null)
                {
                    return true;
                }

                var term = await termRepository.GetByIdAsync(schoolClass.AcademicTermId, cancellationToken);
                return term is null || !term.IsHoliday(request.AttendanceDate);
            })
            .WithMessage("No attendance is expected on a holiday.")
            .WithName("AttendanceDate");
    }
}
