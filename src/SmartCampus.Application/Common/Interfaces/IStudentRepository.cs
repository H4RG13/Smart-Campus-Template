using SmartCampus.Domain.Entities;

namespace SmartCampus.Application.Common.Interfaces;

public interface IStudentRepository
{
    void Add(Student student);
    Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Student>> ListAsync(CancellationToken cancellationToken = default);
    Task<bool> StudentNumberExistsAsync(string studentNumber, CancellationToken cancellationToken = default);
}
