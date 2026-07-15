using SmartCampus.Application.Common.Interfaces;

namespace SmartCampus.Application.Features.Staff;

public sealed class GetStaffListUseCase(IStaffRepository staffRepository)
{
    public async Task<IReadOnlyList<StaffDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var staff = await staffRepository.ListAsync(cancellationToken);
        return staff.Select(CreateStaffUseCase.ToDto).ToList();
    }
}
