using NexusForum.Api.Domain.Entities;

namespace NexusForum.Api.Domain.Interfaces.Repositories;

public interface IPostMemberRepository
{
    Task<bool> IsMemberAsync(int postId, Guid userId);
    Task<PostMember?> GetAsync(int postId, Guid userId);
    Task<List<PostMember>> GetByPostIdAsync(int postId);
    Task AddAsync(PostMember member);
    Task RemoveAsync(PostMember member);
    Task SaveChangesAsync();
}
