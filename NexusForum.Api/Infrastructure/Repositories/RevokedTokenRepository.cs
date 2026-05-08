using Microsoft.EntityFrameworkCore;
using NexusForum.Api.Domain.Entities;
using NexusForum.Api.Domain.Interfaces.Repositories;
using NexusForum.Api.Infrastructure.Data;

namespace NexusForum.Api.Infrastructure.Repositories;

public class RevokedTokenRepository : IRevokedTokenRepository
{
    private readonly AppDbContext _context;

    public RevokedTokenRepository(AppDbContext context) => _context = context;

    public async Task<bool> IsRevokedAsync(string jti) =>
        await _context.RevokedTokens.AnyAsync(r => r.Jti == jti);

    public async Task RevokeAsync(string jti, DateTime expiresAt) =>
        await _context.RevokedTokens.AddAsync(new RevokedToken
        {
            Jti = jti,
            ExpiresAt = expiresAt,
            RevokedAt = DateTime.UtcNow
        });

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
