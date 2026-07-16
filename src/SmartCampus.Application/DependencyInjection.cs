using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SmartCampus.Application.Features.AcademicCalendar;
using SmartCampus.Application.Features.Attendance;
using SmartCampus.Application.Features.Classes;
using SmartCampus.Application.Features.Devices;
using SmartCampus.Application.Features.Guardians;
using SmartCampus.Application.Features.Identity;
using SmartCampus.Application.Features.Notifications;
using SmartCampus.Application.Features.Reports;
using SmartCampus.Application.Features.Reports.Custom;
using SmartCampus.Application.Features.Staff;
using SmartCampus.Application.Features.Students;

namespace SmartCampus.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

        services.AddScoped<LoginUseCase>();

        services.AddScoped<CreateTermUseCase>();
        services.AddScoped<GetTermsUseCase>();
        services.AddScoped<AddCalendarExceptionUseCase>();

        services.AddScoped<CreateClassUseCase>();
        services.AddScoped<GetClassesUseCase>();

        services.AddScoped<CreateStudentUseCase>();
        services.AddScoped<GetStudentsUseCase>();
        services.AddScoped<GetStudentByIdUseCase>();
        services.AddScoped<AssignRfidTagUseCase>();

        services.AddScoped<CreateStaffUseCase>();
        services.AddScoped<GetStaffListUseCase>();

        services.AddScoped<RecordAttendanceUseCase>();
        services.AddScoped<CorrectAttendanceUseCase>();
        services.AddScoped<GetAttendanceRecordsUseCase>();
        services.AddScoped<GetAttendanceSummaryUseCase>();

        services.AddScoped<RegisterDeviceUseCase>();
        services.AddScoped<GetDevicesUseCase>();
        services.AddScoped<SubmitDeviceEventUseCase>();
        services.AddScoped<SubmitHeartbeatUseCase>();
        services.AddScoped<GetDeviceEventsUseCase>();

        services.AddScoped<CreateGuardianUseCase>();
        services.AddScoped<GetGuardiansUseCase>();
        services.AddScoped<LinkGuardianToStudentUseCase>();

        services.AddScoped<AbsenceNotificationService>();
        services.AddScoped<GetNotificationLogsUseCase>();
        services.AddScoped<SendTestNotificationUseCase>();

        services.AddScoped<GetAttendanceReportUseCase>();
        services.AddScoped<GetCustomReportUseCase>();
        // Register ICustomReportHandler implementations here as they're added — see
        // Features/Reports/Custom/README.md. None ship in V1.

        return services;
    }
}
