namespace SmartCampus.Domain.Entities;

public sealed class Student
{
    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string StudentNumber { get; private set; } = null!;
    public Guid ClassId { get; private set; }
    public DateOnly DateOfBirth { get; private set; }
    public string? RfidTagId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime EnrolledAtUtc { get; private set; }
    public DateTime? DeactivatedAtUtc { get; private set; }

    private Student() { }

    public Student(
        string firstName,
        string lastName,
        string studentNumber,
        Guid classId,
        DateOnly dateOfBirth,
        DateTime enrolledAtUtc)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("First name cannot be empty.", nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("Last name cannot be empty.", nameof(lastName));
        }

        if (string.IsNullOrWhiteSpace(studentNumber))
        {
            throw new ArgumentException("Student number cannot be empty.", nameof(studentNumber));
        }

        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        StudentNumber = studentNumber;
        ClassId = classId;
        DateOfBirth = dateOfBirth;
        EnrolledAtUtc = enrolledAtUtc;
        IsActive = true;
    }

    public void ReassignClass(Guid newClassId) => ClassId = newClassId;

    public void AssignRfidTag(string rfidTagId)
    {
        if (string.IsNullOrWhiteSpace(rfidTagId))
        {
            throw new ArgumentException("RFID tag cannot be empty.", nameof(rfidTagId));
        }

        RfidTagId = rfidTagId;
    }

    public void Deactivate(DateTime deactivatedAtUtc)
    {
        IsActive = false;
        DeactivatedAtUtc = deactivatedAtUtc;
    }
}
