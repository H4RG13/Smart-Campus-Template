namespace SmartCampus.Application.Features.Classes;

public sealed class CreateClassRequest
{
    public required string Name { get; init; }
    public required Guid AcademicTermId { get; init; }
}
