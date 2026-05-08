namespace NexusForum.Api.Domain.Interfaces.Repositories;

public interface IRevokedTokenRepository
{
    Task<bool> IsRevokedAsync(string jti);
    Task RevokeAsync(string jti, DateTime expiresAt);
    Task SaveChangesAsync();
}
