using SmartCampus.Application.Common.Interfaces;

namespace SmartCampus.Application.Features.Students;

public sealed class GetStudentsUseCase(IStudentRepository studentRepository)
{
    public async Task<IReadOnlyList<StudentDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var students = await studentRepository.ListAsync(cancellationToken);
        return students.Select(CreateStudentUseCase.ToDto).ToList();
    }
}
