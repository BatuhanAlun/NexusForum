using Microsoft.EntityFrameworkCore;
using NexusForum.Api.Application.DTOs.Comments;
using NexusForum.Api.Domain.Entities;
using NexusForum.Api.Domain.Interfaces.Repositories;
using NexusForum.Api.Infrastructure.Data;

namespace NexusForum.Api.Infrastructure.Repositories;

public class CommentReactionRepository : ICommentReactionRepository
{
    private readonly AppDbContext _context;

    public CommentReactionRepository(AppDbContext context) => _context = context;

    public async Task<CommentReaction?> GetAsync(int commentId, Guid userId) =>
        await _context.CommentReactions
            .FirstOrDefaultAsync(r => r.CommentId == commentId && r.UserId == userId);

    public async Task<ReactionCountDto> GetCountsAsync(int commentId)
    {
        var up = await _context.CommentReactions
            .CountAsync(r => r.CommentId == commentId && r.ReactionType == "up");
        var down = await _context.CommentReactions
            .CountAsync(r => r.CommentId == commentId && r.ReactionType == "down");
        return new ReactionCountDto(up, down, null);
    }

    public async Task AddAsync(CommentReaction reaction) =>
        await _context.CommentReactions.AddAsync(reaction);

    public Task RemoveAsync(CommentReaction reaction)
    {
        _context.CommentReactions.Remove(reaction);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
