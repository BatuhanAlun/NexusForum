using NexusForum.Api.Application.DTOs.Comments;
using NexusForum.Api.Domain.Entities;

namespace NexusForum.Api.Domain.Interfaces.Repositories;

public interface ICommentReactionRepository
{
    Task<CommentReaction?> GetAsync(int commentId, Guid userId);
    Task<ReactionCountDto> GetCountsAsync(int commentId);
    Task AddAsync(CommentReaction reaction);
    Task RemoveAsync(CommentReaction reaction);
    Task SaveChangesAsync();
}
