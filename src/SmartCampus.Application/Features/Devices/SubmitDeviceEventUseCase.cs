using Microsoft.Extensions.Options;
using SmartCampus.Application.Common.Exceptions;
using SmartCampus.Application.Common.Interfaces;
using SmartCampus.Application.Features.Attendance;
using SmartCampus.Configuration;
using SmartCampus.Domain.Entities;
using SmartCampus.Domain.Enums;

namespace SmartCampus.Application.Features.Devices;

/// <summary>
/// The ingestion endpoint for ESP32 scan events (RULES.md #5: every device event is
/// authenticated and validated for plausibility before being trusted). Applies the same
/// AttendanceStatusCalculator as manual entry (Phase 3) — the lateness rule is defined
/// exactly once, never duplicated in firmware.
/// </summary>
public sealed class SubmitDeviceEventUseCase(
    IDeviceRepository deviceRepository,
    IDeviceEventRepository deviceEventRepository,
    IStudentRepository studentRepository,
    IAttendanceRecordRepository attendanceRecordRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    IOptions<SchoolSettings> schoolSettings)
{
    private static readonly TimeSpan MaxFutureSkew = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan MaxPastSkew = TimeSpan.FromHours(24);

    public async Task<SubmitDeviceEventResponse> ExecuteAsync(
        Guid deviceId,
        string deviceAuthToken,
        SubmitDeviceEventRequest request,
        CancellationToken cancellationToken = default)
    {
        var device = await deviceRepository.GetByIdAsync(deviceId, cancellationToken);
        if (device is null || !device.IsActive || !passwordHasher.Verify(deviceAuthToken, device.AuthTokenHash))
        {
            throw new DeviceAuthenticationFailedException();
        }

        if (await deviceEventRepository.ExistsAsync(deviceId, request.DeviceEventId, cancellationToken))
        {
            return new SubmitDeviceEventResponse { ProcessingStatus = DeviceEventProcessingStatus.RejectedDuplicate };
        }

        var now = DateTime.UtcNow;
        if (request.DeviceTimestampUtc > now + MaxFutureSkew || request.DeviceTimestampUtc < now - MaxPastSkew)
        {
            await RecordEventAsync(deviceId, request, DeviceEventProcessingStatus.RejectedInvalidTimestamp, now, cancellationToken);
            return new SubmitDeviceEventResponse { ProcessingStatus = DeviceEventProcessingStatus.RejectedInvalidTimestamp };
        }

        var student = await studentRepository.GetByRfidTagAsync(request.RfidTagId, cancellationToken);
        if (student is null)
        {
            await RecordEventAsync(deviceId, request, DeviceEventProcessingStatus.RejectedUnknownTag, now, cancellationToken);
            return new SubmitDeviceEventResponse { ProcessingStatus = DeviceEventProcessingStatus.RejectedUnknownTag };
        }

        var deviceEvent = new DeviceEvent(deviceId, request.DeviceEventId, request.RfidTagId, request.DeviceTimestampUtc, now, DeviceEventProcessingStatus.Accepted);
        deviceEventRepository.Add(deviceEvent);

        var attendanceDate = DateOnly.FromDateTime(request.DeviceTimestampUtc);
        var existingRecord = await attendanceRecordRepository.GetByStudentAndDateAsync(student.Id, attendanceDate, cancellationToken);

        AttendanceStatus attendanceStatus;
        if (existingRecord is null)
        {
            var checkInTime = TimeOnly.FromDateTime(request.DeviceTimestampUtc);
            attendanceStatus = AttendanceStatusCalculator.Calculate(checkInTime, schoolSettings.Value);

            var attendanceRecord = AttendanceRecord.FromDeviceEvent(
                student.Id, attendanceDate, attendanceStatus, request.DeviceTimestampUtc, deviceEvent.Id, now);
            attendanceRecordRepository.Add(attendanceRecord);
        }
        else
        {
            attendanceStatus = existingRecord.Status;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new SubmitDeviceEventResponse
        {
            ProcessingStatus = DeviceEventProcessingStatus.Accepted,
            AttendanceStatus = attendanceStatus,
            StudentDisplayName = $"{student.FirstName[0]}. {student.LastName}",
        };
    }

    private async Task RecordEventAsync(
        Guid deviceId,
        SubmitDeviceEventRequest request,
        DeviceEventProcessingStatus status,
        DateTime receivedAtUtc,
        CancellationToken cancellationToken)
    {
        deviceEventRepository.Add(new DeviceEvent(deviceId, request.DeviceEventId, request.RfidTagId, request.DeviceTimestampUtc, receivedAtUtc, status));
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
