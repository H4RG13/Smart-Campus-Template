using Microsoft.EntityFrameworkCore;
using SmartCampus.Application.Common.Interfaces;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Infrastructure.Persistence.Repositories;

public sealed class AcademicTermRepository(AppDbContext dbContext) : IAcademicTermRepository
{
    public void Add(AcademicTerm term) => dbContext.AcademicTerms.Add(term);

    public void AddException(CalendarException exception) => dbContext.CalendarExceptions.Add(exception);

    public Task<AcademicTerm?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.AcademicTerms
            .Include(t => t.Exceptions)
            .SingleOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<AcademicTerm>> ListAsync(CancellationToken cancellationToken = default) =>
        await dbContext.AcademicTerms
            .Include(t => t.Exceptions)
            .OrderByDescending(t => t.StartDate)
            .ToListAsync(cancellationToken);
}
