namespace SmartCampus.Configuration;

public sealed class NotificationSettings
{
    public const string SectionName = "Notifications";

    /// <summary>Per-school toggle: whether an unexcused absence emails the student's guardians.</summary>
    public required bool NotifyGuardiansOnAbsence { get; init; }
}
