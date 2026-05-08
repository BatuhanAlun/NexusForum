using NexusForum.Api.Application.DTOs.Posts;
using NexusForum.Api.Common.Results;

namespace NexusForum.Api.Application.Interfaces.Services;

public interface IPrivateThreadService
{
    Task<Result<PostMemberDto>> InviteAsync(int postId, Guid requesterId, string username);
    Task<Result<bool>> RemoveMemberAsync(int postId, Guid requesterId, Guid userId);
    Task<Result<IReadOnlyList<PostMemberDto>>> GetMembersAsync(int postId, Guid requesterId);
}
