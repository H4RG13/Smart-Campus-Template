namespace SmartCampus.Domain.Entities;

public sealed class Staff
{
    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string EmployeeNumber { get; private set; } = null!;
    public string Position { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;

    private Staff() { }

    public Staff(string firstName, string lastName, string employeeNumber, string position)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("First name cannot be empty.", nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("Last name cannot be empty.", nameof(lastName));
        }

        if (string.IsNullOrWhiteSpace(employeeNumber))
        {
            throw new ArgumentException("Employee number cannot be empty.", nameof(employeeNumber));
        }

        if (string.IsNullOrWhiteSpace(position))
        {
            throw new ArgumentException("Position cannot be empty.", nameof(position));
        }

        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        EmployeeNumber = employeeNumber;
        Position = position;
        IsActive = true;
    }

    public void Deactivate() => IsActive = false;
}
