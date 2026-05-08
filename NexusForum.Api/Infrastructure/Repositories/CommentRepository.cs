using Microsoft.EntityFrameworkCore;
using NexusForum.Api.Domain.Entities;
using NexusForum.Api.Domain.Interfaces.Repositories;
using NexusForum.Api.Infrastructure.Data;

namespace NexusForum.Api.Infrastructure.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly AppDbContext _context;

    public CommentRepository(AppDbContext context) => _context = context;

    public async Task<Comment?> GetByIdAsync(int id) =>
        await _context.Comments
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<int> CountByAuthorAsync(Guid authorId) =>
        await _context.Comments.CountAsync(c => c.AuthorId == authorId);

    public async Task AddAsync(Comment comment) =>
        await _context.Comments.AddAsync(comment);

    public async Task DeleteAsync(Comment comment) =>
        _context.Comments.Remove(comment);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
