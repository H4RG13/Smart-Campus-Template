namespace SmartCampus.Domain.Entities;

public sealed class Role
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;

    private Role() { }

    public Role(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Role name cannot be empty.", nameof(name));
        }

        Id = Guid.NewGuid();
        Name = name;
    }
}
