namespace SmartCampus.Domain.Enums;

public enum DeviceEventProcessingStatus
{
    Accepted,
    RejectedDuplicate,
    RejectedUnknownTag,
    RejectedInvalidTimestamp,
}
