namespace SmartCampus.Application.Features.Students;

public sealed class StudentDto
{
    public required Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string StudentNumber { get; init; }
    public required Guid ClassId { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public string? RfidTagId { get; init; }
    public required bool IsActive { get; init; }
}
