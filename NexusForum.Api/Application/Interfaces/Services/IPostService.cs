using NexusForum.Api.Application.DTOs.Posts;
using NexusForum.Api.Common.Pagination;
using NexusForum.Api.Common.Results;

namespace NexusForum.Api.Application.Interfaces.Services;

public interface IPostService
{
    Task<PagedResult<PostSummaryDto>> GetPagedAsync(int page, int pageSize, int? categoryId = null, Guid? viewerUserId = null);
    Task<Result<PostDto>> GetByIdAsync(int id, Guid? viewerUserId = null);
    Task<Result<PostDto>> CreateAsync(CreatePostRequest request, Guid authorId);
    Task<Result<PostDto>> UpdateAsync(int id, UpdatePostRequest request, Guid requesterId, bool isAdmin);
    Task<Result<bool>> DeleteAsync(int id, Guid requesterId, bool isAdmin);
}
