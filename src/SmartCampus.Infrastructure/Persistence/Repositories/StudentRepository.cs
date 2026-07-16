using Microsoft.EntityFrameworkCore;
using SmartCampus.Application.Common.Interfaces;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Infrastructure.Persistence.Repositories;

public sealed class StudentRepository(AppDbContext dbContext) : IStudentRepository
{
    public void Add(Student student) => dbContext.Students.Add(student);

    public Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Students.SingleOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Student>> ListAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Students.OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ToListAsync(cancellationToken);

    public Task<bool> StudentNumberExistsAsync(string studentNumber, CancellationToken cancellationToken = default) =>
        dbContext.Students.AnyAsync(s => s.StudentNumber == studentNumber, cancellationToken);

    public Task<Student?> GetByRfidTagAsync(string rfidTagId, CancellationToken cancellationToken = default) =>
        dbContext.Students.SingleOrDefaultAsync(s => s.RfidTagId == rfidTagId, cancellationToken);
}
