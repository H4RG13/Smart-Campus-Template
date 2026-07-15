using SmartCampus.Domain.Enums;

namespace SmartCampus.Domain.Entities;

public sealed class CalendarException
{
    public Guid Id { get; private set; }
    public Guid AcademicTermId { get; private set; }
    public DateOnly Date { get; private set; }
    public CalendarExceptionType Type { get; private set; }
    public string? Description { get; private set; }

    private CalendarException() { }

    internal CalendarException(Guid academicTermId, DateOnly date, CalendarExceptionType type, string? description)
    {
        Id = Guid.NewGuid();
        AcademicTermId = academicTermId;
        Date = date;
        Type = type;
        Description = description;
    }
}
