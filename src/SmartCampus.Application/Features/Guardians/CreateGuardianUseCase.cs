using SmartCampus.Application.Common.Interfaces;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Application.Features.Guardians;

public sealed class CreateGuardianUseCase(IGuardianRepository guardianRepository, IUnitOfWork unitOfWork)
{
    public async Task<GuardianDto> ExecuteAsync(CreateGuardianRequest request, CancellationToken cancellationToken = default)
    {
        var guardian = new Guardian(request.FirstName, request.LastName, request.Phone, request.Email);
        guardianRepository.Add(guardian);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(guardian);
    }

    internal static GuardianDto ToDto(Guardian guardian) => new()
    {
        Id = guardian.Id,
        FirstName = guardian.FirstName,
        LastName = guardian.LastName,
        Phone = guardian.Phone,
        Email = guardian.Email,
        IsActive = guardian.IsActive,
        StudentLinks = guardian.StudentLinks.Select(l => new GuardianStudentLinkDto
        {
            StudentId = l.StudentId,
            Relationship = l.Relationship,
        }).ToList(),
    };
}
