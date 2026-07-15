using Microsoft.EntityFrameworkCore;
using SmartCampus.Application.Common.Interfaces;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Infrastructure.Persistence.Repositories;

public sealed class ClassRepository(AppDbContext dbContext) : IClassRepository
{
    public void Add(SchoolClass schoolClass) => dbContext.Classes.Add(schoolClass);

    public Task<SchoolClass?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Classes.SingleOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<SchoolClass>> ListAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Classes.OrderBy(c => c.Name).ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Classes.AnyAsync(c => c.Id == id, cancellationToken);
}
