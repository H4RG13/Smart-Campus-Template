namespace SmartCampus.Domain.Entities;

public sealed class User
{
    private readonly List<Role> _roles = [];

    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? DeactivatedAtUtc { get; private set; }

    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

    private User() { }

    public User(string email, string passwordHash, DateTime createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));
        }

        Id = Guid.NewGuid();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
    }

    public void AssignRole(Role role)
    {
        if (_roles.Any(r => r.Id == role.Id))
        {
            return;
        }

        _roles.Add(role);
    }

    public void Deactivate(DateTime deactivatedAtUtc)
    {
        IsActive = false;
        DeactivatedAtUtc = deactivatedAtUtc;
    }
}
