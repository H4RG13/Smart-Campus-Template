using SmartCampus.Domain.Entities;

namespace SmartCampus.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
