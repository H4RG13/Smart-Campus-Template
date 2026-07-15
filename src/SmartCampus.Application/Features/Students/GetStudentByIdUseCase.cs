using SmartCampus.Application.Common.Interfaces;

namespace SmartCampus.Application.Features.Students;

public sealed class GetStudentByIdUseCase(IStudentRepository studentRepository)
{
    public async Task<StudentDto?> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var student = await studentRepository.GetByIdAsync(id, cancellationToken);
        return student is null ? null : CreateStudentUseCase.ToDto(student);
    }
}
