using NexusForum.Api.Domain.Entities;

namespace NexusForum.Api.Domain.Interfaces.Repositories;

// Interface lives in Domain so Application can depend on it without touching Infrastructure.
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByUsernameAsync(string username);
    Task AddAsync(User user);
    Task SaveChangesAsync();
}
