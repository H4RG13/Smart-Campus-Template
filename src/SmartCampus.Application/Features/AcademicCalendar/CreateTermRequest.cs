namespace SmartCampus.Application.Features.AcademicCalendar;

public sealed class CreateTermRequest
{
    public required string Name { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
}
