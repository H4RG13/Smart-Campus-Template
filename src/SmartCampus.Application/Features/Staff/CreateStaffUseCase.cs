using SmartCampus.Application.Common.Interfaces;
using DomainStaff = SmartCampus.Domain.Entities.Staff;

namespace SmartCampus.Application.Features.Staff;

public sealed class CreateStaffUseCase(IStaffRepository staffRepository, IUnitOfWork unitOfWork)
{
    public async Task<StaffDto> ExecuteAsync(CreateStaffRequest request, CancellationToken cancellationToken = default)
    {
        var staff = new DomainStaff(request.FirstName, request.LastName, request.EmployeeNumber, request.Position);
        staffRepository.Add(staff);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(staff);
    }

    internal static StaffDto ToDto(DomainStaff staff) => new()
    {
        Id = staff.Id,
        FirstName = staff.FirstName,
        LastName = staff.LastName,
        EmployeeNumber = staff.EmployeeNumber,
        Position = staff.Position,
        IsActive = staff.IsActive,
    };
}
