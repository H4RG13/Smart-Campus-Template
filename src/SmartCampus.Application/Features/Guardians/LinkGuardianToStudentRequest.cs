namespace SmartCampus.Application.Features.Guardians;

public sealed class LinkGuardianToStudentRequest
{
    public required Guid StudentId { get; init; }
    public required string Relationship { get; init; }
}
