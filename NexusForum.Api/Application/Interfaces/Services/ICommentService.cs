using NexusForum.Api.Application.DTOs.Comments;
using NexusForum.Api.Common.Results;

namespace NexusForum.Api.Application.Interfaces.Services;

public interface ICommentService
{
    Task<Result<CommentDto>> CreateAsync(int postId, CreateCommentRequest request, Guid authorId);
    Task<Result<CommentDto>> UpdateAsync(int id, UpdateCommentRequest request, Guid requesterId, bool isAdmin);
    Task<Result<bool>> DeleteAsync(int id, Guid requesterId, bool isAdmin);
}
