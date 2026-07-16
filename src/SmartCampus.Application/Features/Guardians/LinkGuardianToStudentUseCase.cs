using SmartCampus.Application.Common.Interfaces;

namespace SmartCampus.Application.Features.Guardians;

public sealed class LinkGuardianToStudentUseCase(IGuardianRepository guardianRepository, IUnitOfWork unitOfWork)
{
    public async Task<GuardianDto?> ExecuteAsync(Guid guardianId, LinkGuardianToStudentRequest request, CancellationToken cancellationToken = default)
    {
        var guardian = await guardianRepository.GetByIdAsync(guardianId, cancellationToken);
        if (guardian is null)
        {
            return null;
        }

        var link = guardian.LinkToStudent(request.StudentId, request.Relationship);
        guardianRepository.AddStudentLink(link);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateGuardianUseCase.ToDto(guardian);
    }
}
