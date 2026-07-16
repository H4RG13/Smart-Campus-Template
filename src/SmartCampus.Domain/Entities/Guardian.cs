namespace SmartCampus.Domain.Entities;

public sealed class Guardian
{
    private readonly List<GuardianStudent> _studentLinks = [];

    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<GuardianStudent> StudentLinks => _studentLinks.AsReadOnly();

    private Guardian() { }

    public Guardian(string firstName, string lastName, string phone, string email)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("First name cannot be empty.", nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("Last name cannot be empty.", nameof(lastName));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        }

        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        Email = email.Trim().ToLowerInvariant();
        IsActive = true;
    }

    public GuardianStudent LinkToStudent(Guid studentId, string relationship)
    {
        if (_studentLinks.Any(l => l.StudentId == studentId))
        {
            throw new InvalidOperationException("This guardian is already linked to this student.");
        }

        var link = new GuardianStudent(Id, studentId, relationship);
        _studentLinks.Add(link);
        return link;
    }

    public void Deactivate() => IsActive = false;
}
