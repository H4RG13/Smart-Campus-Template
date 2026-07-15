using SmartCampus.Application.Common.Interfaces;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Application.Features.Students;

public sealed class CreateStudentUseCase(IStudentRepository studentRepository, IUnitOfWork unitOfWork)
{
    public async Task<StudentDto> ExecuteAsync(CreateStudentRequest request, CancellationToken cancellationToken = default)
    {
        var student = new Student(
            request.FirstName,
            request.LastName,
            request.StudentNumber,
            request.ClassId,
            request.DateOfBirth,
            DateTime.UtcNow);

        studentRepository.Add(student);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(student);
    }

    internal static StudentDto ToDto(Student student) => new()
    {
        Id = student.Id,
        FirstName = student.FirstName,
        LastName = student.LastName,
        StudentNumber = student.StudentNumber,
        ClassId = student.ClassId,
        DateOfBirth = student.DateOfBirth,
        RfidTagId = student.RfidTagId,
        IsActive = student.IsActive,
    };
}
