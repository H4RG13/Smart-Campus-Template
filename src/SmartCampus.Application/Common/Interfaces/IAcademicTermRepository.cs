using SmartCampus.Domain.Entities;

namespace SmartCampus.Application.Common.Interfaces;

public interface IAcademicTermRepository
{
    void Add(AcademicTerm term);
    void AddException(CalendarException exception);
    Task<AcademicTerm?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AcademicTerm>> ListAsync(CancellationToken cancellationToken = default);
}
