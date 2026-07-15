namespace SmartCampus.Application.Features.Staff;

public sealed class StaffDto
{
    public required Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string EmployeeNumber { get; init; }
    public required string Position { get; init; }
    public required bool IsActive { get; init; }
}
