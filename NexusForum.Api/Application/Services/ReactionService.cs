using NexusForum.Api.Application.DTOs.Comments;
using NexusForum.Api.Application.Interfaces.Services;
using NexusForum.Api.Common.Results;
using NexusForum.Api.Domain.Entities;
using NexusForum.Api.Domain.Interfaces.Repositories;

namespace NexusForum.Api.Application.Services;

public class ReactionService : IReactionService
{
    private readonly ICommentReactionRepository _reactionRepo;
    private readonly ICommentRepository _commentRepo;

    public ReactionService(ICommentReactionRepository reactionRepo, ICommentRepository commentRepo)
    {
        _reactionRepo = reactionRepo;
        _commentRepo = commentRepo;
    }

    public async Task<Result<ReactionCountDto>> ReactAsync(int commentId, Guid userId, string reactionType)
    {
        if (reactionType is not ("up" or "down"))
            return Result<ReactionCountDto>.Failure("Invalid reaction type. Use 'up' or 'down'.", 400);

        var comment = await _commentRepo.GetByIdAsync(commentId);
        if (comment is null) return Result<ReactionCountDto>.Failure("Comment not found.", 404);

        // INTENTIONAL VULNERABILITY (CWE-362 TOCTOU):
        // CHECK: has this user already reacted?
        var existing = await _reactionRepo.GetAsync(commentId, userId);

        string? myReaction;

        if (existing is not null && existing.ReactionType == reactionType)
        {
            // Same type → undo (remove)
            await _reactionRepo.RemoveAsync(existing);
            myReaction = null;
        }
        else if (existing is not null)
        {
            // Different type → switch reaction
            existing.ReactionType = reactionType;
            myReaction = reactionType;
        }
        else
        {
            // TIME WINDOW: concurrent requests all pass the check above before any has committed.
            // Without a DB-level unique constraint on (CommentId, UserId), all will insert successfully.

            // ACT: insert new reaction
            await _reactionRepo.AddAsync(new CommentReaction
            {
                CommentId = commentId,
                UserId = userId,
                ReactionType = reactionType,
                CreatedAt = DateTime.UtcNow,
            });
            myReaction = reactionType;
        }

        await _reactionRepo.SaveChangesAsync();

        var counts = await _reactionRepo.GetCountsAsync(commentId);
        return Result<ReactionCountDto>.Success(counts with { MyReaction = myReaction });
    }
}
