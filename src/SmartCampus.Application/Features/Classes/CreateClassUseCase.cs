using SmartCampus.Application.Common.Interfaces;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Application.Features.Classes;

public sealed class CreateClassUseCase(IClassRepository classRepository, IUnitOfWork unitOfWork)
{
    public async Task<ClassDto> ExecuteAsync(CreateClassRequest request, CancellationToken cancellationToken = default)
    {
        var schoolClass = new SchoolClass(request.Name, request.AcademicTermId);
        classRepository.Add(schoolClass);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ClassDto
        {
            Id = schoolClass.Id,
            Name = schoolClass.Name,
            AcademicTermId = schoolClass.AcademicTermId,
        };
    }
}
