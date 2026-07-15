namespace SmartCampus.Application.Features.Classes;

public sealed class ClassDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required Guid AcademicTermId { get; init; }
}
