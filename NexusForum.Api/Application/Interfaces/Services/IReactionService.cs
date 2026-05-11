using NexusForum.Api.Application.DTOs.Comments;
using NexusForum.Api.Common.Results;

namespace NexusForum.Api.Application.Interfaces.Services;

public interface IReactionService
{
    Task<Result<ReactionCountDto>> ReactAsync(int commentId, Guid userId, string reactionType);
}
