using NexusForum.Api.Domain.Entities;

namespace NexusForum.Api.Domain.Interfaces.Repositories;

public interface ICommentRepository
{
    Task<Comment?> GetByIdAsync(int id);
    Task<int> CountByAuthorAsync(Guid authorId);
    Task AddAsync(Comment comment);
    Task DeleteAsync(Comment comment);
    Task SaveChangesAsync();
}
