namespace SmartCampus.Domain.Entities;

public sealed class GuardianStudent
{
    public Guid Id { get; private set; }
    public Guid GuardianId { get; private set; }
    public Guid StudentId { get; private set; }
    public string Relationship { get; private set; } = null!;

    private GuardianStudent() { }

    internal GuardianStudent(Guid guardianId, Guid studentId, string relationship)
    {
        Id = Guid.NewGuid();
        GuardianId = guardianId;
        StudentId = studentId;
        Relationship = relationship;
    }
}
