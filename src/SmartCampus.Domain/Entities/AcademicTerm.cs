namespace SmartCampus.Domain.Entities;

public sealed class AcademicTerm
{
    private readonly List<CalendarException> _exceptions = [];

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<CalendarException> Exceptions => _exceptions.AsReadOnly();

    private AcademicTerm() { }

    public AcademicTerm(string name, DateOnly startDate, DateOnly endDate)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Term name cannot be empty.", nameof(name));
        }

        if (endDate <= startDate)
        {
            throw new ArgumentException("End date must be after start date.", nameof(endDate));
        }

        Id = Guid.NewGuid();
        Name = name;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = true;
    }

    public CalendarException AddException(DateOnly date, Enums.CalendarExceptionType type, string? description)
    {
        if (date < StartDate || date > EndDate)
        {
            throw new ArgumentException("Exception date must fall within the term's date range.", nameof(date));
        }

        var exception = new CalendarException(Id, date, type, description);
        _exceptions.Add(exception);
        return exception;
    }

    public void Deactivate() => IsActive = false;

    public bool IsHoliday(DateOnly date) =>
        _exceptions.Any(e => e.Date == date && e.Type == Enums.CalendarExceptionType.Holiday);
}
