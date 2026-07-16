using SmartCampus.Application.Common.Interfaces;

namespace SmartCampus.Application.Features.Guardians;

public sealed class GetGuardiansUseCase(IGuardianRepository guardianRepository)
{
    public async Task<IReadOnlyList<GuardianDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var guardians = await guardianRepository.ListAsync(cancellationToken);
        return guardians.Select(CreateGuardianUseCase.ToDto).ToList();
    }
}
