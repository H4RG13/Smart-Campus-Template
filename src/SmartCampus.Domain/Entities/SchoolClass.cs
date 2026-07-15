namespace SmartCampus.Domain.Entities;

public sealed class SchoolClass
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public Guid AcademicTermId { get; private set; }

    private SchoolClass() { }

    public SchoolClass(string name, Guid academicTermId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Class name cannot be empty.", nameof(name));
        }

        Id = Guid.NewGuid();
        Name = name;
        AcademicTermId = academicTermId;
    }
}
