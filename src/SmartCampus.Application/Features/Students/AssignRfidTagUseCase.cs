using SmartCampus.Application.Common.Interfaces;

namespace SmartCampus.Application.Features.Students;

public sealed class AssignRfidTagUseCase(IStudentRepository studentRepository, IUnitOfWork unitOfWork)
{
    public async Task<StudentDto?> ExecuteAsync(Guid studentId, AssignRfidTagRequest request, CancellationToken cancellationToken = default)
    {
        var student = await studentRepository.GetByIdAsync(studentId, cancellationToken);
        if (student is null)
        {
            return null;
        }

        student.AssignRfidTag(request.RfidTagId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateStudentUseCase.ToDto(student);
    }
}
