namespace SmartCampus.Application.Features.Staff;

public sealed class CreateStaffRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string EmployeeNumber { get; init; }
    public required string Position { get; init; }
}
