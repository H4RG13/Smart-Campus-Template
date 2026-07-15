using SmartCampus.Application.Common.Interfaces;

namespace SmartCampus.Application.Features.AcademicCalendar;

public sealed class AddCalendarExceptionUseCase(IAcademicTermRepository termRepository, IUnitOfWork unitOfWork)
{
    public async Task<CalendarExceptionDto> ExecuteAsync(
        AddCalendarExceptionRequest request,
        CancellationToken cancellationToken = default)
    {
        var term = await termRepository.GetByIdAsync(request.AcademicTermId, cancellationToken)
            ?? throw new InvalidOperationException("Academic term does not exist.");

        var exception = term.AddException(request.Date, request.Type, request.Description);
        termRepository.AddException(exception);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CalendarExceptionDto
        {
            Id = exception.Id,
            AcademicTermId = exception.AcademicTermId,
            Date = exception.Date,
            Type = exception.Type,
            Description = exception.Description,
        };
    }
}
