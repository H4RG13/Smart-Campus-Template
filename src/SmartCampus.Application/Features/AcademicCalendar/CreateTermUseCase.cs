using SmartCampus.Application.Common.Interfaces;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Application.Features.AcademicCalendar;

public sealed class CreateTermUseCase(IAcademicTermRepository termRepository, IUnitOfWork unitOfWork)
{
    public async Task<TermDto> ExecuteAsync(CreateTermRequest request, CancellationToken cancellationToken = default)
    {
        var term = new AcademicTerm(request.Name, request.StartDate, request.EndDate);
        termRepository.Add(term);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new TermDto
        {
            Id = term.Id,
            Name = term.Name,
            StartDate = term.StartDate,
            EndDate = term.EndDate,
            IsActive = term.IsActive,
        };
    }
}
