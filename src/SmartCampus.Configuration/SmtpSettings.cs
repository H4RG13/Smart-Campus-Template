namespace SmartCampus.Configuration;

public sealed class SmtpSettings
{
    public const string SectionName = "Smtp";

    public required string Host { get; init; }
    public required int Port { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
    public required bool UseSsl { get; init; }
    public required string FromAddress { get; init; }
    public required string FromName { get; init; }
}
