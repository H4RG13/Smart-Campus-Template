using SmartCampus.Domain.Enums;

namespace SmartCampus.Application.Features.AcademicCalendar;

public sealed class CalendarExceptionDto
{
    public required Guid Id { get; init; }
    public required Guid AcademicTermId { get; init; }
    public required DateOnly Date { get; init; }
    public required CalendarExceptionType Type { get; init; }
    public string? Description { get; init; }
}
