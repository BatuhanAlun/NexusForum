using NexusForum.Api.Domain.Entities;

namespace NexusForum.Api.Domain.Interfaces.Repositories;

public interface IPostRepository
{
    Task<(List<Post> Posts, int TotalCount)> GetPagedAsync(int page, int pageSize, int? categoryId = null, Guid? authorId = null, Guid? viewerUserId = null);
    Task<Post?> GetByIdAsync(int id);
    Task<Post?> GetByIdWithDetailsAsync(int id);
    Task<int> CountByAuthorAsync(Guid authorId);
    Task AddAsync(Post post);
    Task DeleteAsync(Post post);
    Task SaveChangesAsync();
}
