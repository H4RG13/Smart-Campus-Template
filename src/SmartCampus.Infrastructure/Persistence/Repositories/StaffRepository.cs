using Microsoft.EntityFrameworkCore;
using SmartCampus.Application.Common.Interfaces;
using SmartCampus.Domain.Entities;

namespace SmartCampus.Infrastructure.Persistence.Repositories;

public sealed class StaffRepository(AppDbContext dbContext) : IStaffRepository
{
    public void Add(Staff staff) => dbContext.Staff.Add(staff);

    public Task<Staff?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Staff.SingleOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Staff>> ListAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Staff.OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ToListAsync(cancellationToken);

    public Task<bool> EmployeeNumberExistsAsync(string employeeNumber, CancellationToken cancellationToken = default) =>
        dbContext.Staff.AnyAsync(s => s.EmployeeNumber == employeeNumber, cancellationToken);
}
