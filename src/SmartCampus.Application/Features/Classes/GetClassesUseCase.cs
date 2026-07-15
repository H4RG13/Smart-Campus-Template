using SmartCampus.Application.Common.Interfaces;

namespace SmartCampus.Application.Features.Classes;

public sealed class GetClassesUseCase(IClassRepository classRepository)
{
    public async Task<IReadOnlyList<ClassDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var classes = await classRepository.ListAsync(cancellationToken);

        return classes.Select(c => new ClassDto
        {
            Id = c.Id,
            Name = c.Name,
            AcademicTermId = c.AcademicTermId,
        }).ToList();
    }
}
