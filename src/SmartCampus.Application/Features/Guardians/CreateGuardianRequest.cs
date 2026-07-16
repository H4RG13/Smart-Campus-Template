namespace SmartCampus.Application.Features.Guardians;

public sealed class CreateGuardianRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Phone { get; init; }
    public required string Email { get; init; }
}
