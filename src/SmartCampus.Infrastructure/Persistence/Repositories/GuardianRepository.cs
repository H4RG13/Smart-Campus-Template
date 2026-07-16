using Microsoft.EntityFrameworkCore;
using SmartCampus.Application.Common.Interfaces;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Infrastructure.Persistence.Repositories;

public sealed class GuardianRepository(AppDbContext dbContext) : IGuardianRepository
{
    public void Add(Guardian guardian) => dbContext.Guardians.Add(guardian);

    public void AddStudentLink(GuardianStudent link) => dbContext.GuardianStudents.Add(link);

    public Task<Guardian?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Guardians
            .Include(g => g.StudentLinks)
            .SingleOrDefaultAsync(g => g.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Guardian>> ListAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Guardians
            .Include(g => g.StudentLinks)
            .OrderBy(g => g.LastName).ThenBy(g => g.FirstName)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Guardian>> ListByStudentAsync(Guid studentId, CancellationToken cancellationToken = default) =>
        await dbContext.Guardians
            .Include(g => g.StudentLinks)
            .Where(g => g.StudentLinks.Any(l => l.StudentId == studentId))
            .ToListAsync(cancellationToken);
}
