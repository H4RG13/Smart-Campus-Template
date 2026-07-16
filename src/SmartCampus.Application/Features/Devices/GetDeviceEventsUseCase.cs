using SmartCampus.Application.Common.Interfaces;

namespace SmartCampus.Application.Features.Devices;

public sealed class GetDeviceEventsUseCase(IDeviceEventRepository deviceEventRepository)
{
    public async Task<IReadOnlyList<DeviceEventDto>> ExecuteAsync(Guid deviceId, CancellationToken cancellationToken = default)
    {
        var events = await deviceEventRepository.ListByDeviceAsync(deviceId, cancellationToken);

        return events.Select(e => new DeviceEventDto
        {
            Id = e.Id,
            DeviceEventId = e.DeviceEventId,
            RfidTagId = e.RfidTagId,
            DeviceTimestampUtc = e.DeviceTimestampUtc,
            ReceivedAtUtc = e.ReceivedAtUtc,
            ProcessingStatus = e.ProcessingStatus,
        }).ToList();
    }
}
