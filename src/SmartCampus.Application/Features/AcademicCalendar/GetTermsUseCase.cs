using SmartCampus.Application.Common.Interfaces;

namespace SmartCampus.Application.Features.AcademicCalendar;

public sealed class GetTermsUseCase(IAcademicTermRepository termRepository)
{
    public async Task<IReadOnlyList<TermDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var terms = await termRepository.ListAsync(cancellationToken);

        return terms.Select(term => new TermDto
        {
            Id = term.Id,
            Name = term.Name,
            StartDate = term.StartDate,
            EndDate = term.EndDate,
            IsActive = term.IsActive,
        }).ToList();
    }
}
