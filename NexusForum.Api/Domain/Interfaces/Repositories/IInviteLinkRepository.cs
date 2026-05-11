using NexusForum.Api.Domain.Entities;

namespace NexusForum.Api.Domain.Interfaces.Repositories;

public interface IInviteLinkRepository
{
    Task<InviteLink?> GetByTokenAsync(string token);
    Task AddAsync(InviteLink link);
    Task SaveChangesAsync();
}
