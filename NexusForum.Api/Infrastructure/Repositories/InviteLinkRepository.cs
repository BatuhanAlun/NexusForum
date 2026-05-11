using Microsoft.EntityFrameworkCore;
using NexusForum.Api.Domain.Entities;
using NexusForum.Api.Domain.Interfaces.Repositories;
using NexusForum.Api.Infrastructure.Data;

namespace NexusForum.Api.Infrastructure.Repositories;

public class InviteLinkRepository : IInviteLinkRepository
{
    private readonly AppDbContext _context;

    public InviteLinkRepository(AppDbContext context) => _context = context;

    public async Task<InviteLink?> GetByTokenAsync(string token) =>
        await _context.InviteLinks.FirstOrDefaultAsync(l => l.Token == token);

    public async Task AddAsync(InviteLink link) =>
        await _context.InviteLinks.AddAsync(link);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
