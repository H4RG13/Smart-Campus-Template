using SmartCampus.Domain.Entities;

namespace SmartCampus.Application.Common.Interfaces;

public interface IGuardianRepository
{
    void Add(Guardian guardian);
    void AddStudentLink(GuardianStudent link);
    Task<Guardian?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guardian>> ListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guardian>> ListByStudentAsync(Guid studentId, CancellationToken cancellationToken = default);
}
