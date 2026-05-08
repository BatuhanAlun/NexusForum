using Microsoft.EntityFrameworkCore;
using NexusForum.Api.Domain.Entities;
using NexusForum.Api.Domain.Interfaces.Repositories;
using NexusForum.Api.Infrastructure.Data;

namespace NexusForum.Api.Infrastructure.Repositories;

public class PostMemberRepository : IPostMemberRepository
{
    private readonly AppDbContext _context;

    public PostMemberRepository(AppDbContext context) => _context = context;

    public async Task<bool> IsMemberAsync(int postId, Guid userId) =>
        await _context.PostMembers.AnyAsync(m => m.PostId == postId && m.UserId == userId);

    public async Task<PostMember?> GetAsync(int postId, Guid userId) =>
        await _context.PostMembers.FirstOrDefaultAsync(m => m.PostId == postId && m.UserId == userId);

    public async Task<List<PostMember>> GetByPostIdAsync(int postId) =>
        await _context.PostMembers
            .Include(m => m.User)
            .Where(m => m.PostId == postId)
            .OrderBy(m => m.InvitedAt)
            .ToListAsync();

    public async Task AddAsync(PostMember member) =>
        await _context.PostMembers.AddAsync(member);

    public async Task RemoveAsync(PostMember member) =>
        _context.PostMembers.Remove(member);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
