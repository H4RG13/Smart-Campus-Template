namespace SmartCampus.Application.Features.Guardians;

public sealed class GuardianDto
{
    public required Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Phone { get; init; }
    public required string Email { get; init; }
    public required bool IsActive { get; init; }
    public required IReadOnlyList<GuardianStudentLinkDto> StudentLinks { get; init; }
}

public sealed class GuardianStudentLinkDto
{
    public required Guid StudentId { get; init; }
    public required string Relationship { get; init; }
}
