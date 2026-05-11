using Microsoft.EntityFrameworkCore;
using NexusForum.Api.Domain.Entities;
using NexusForum.Api.Domain.Interfaces.Repositories;
using NexusForum.Api.Infrastructure.Data;

namespace NexusForum.Api.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly AppDbContext _context;

    public PostRepository(AppDbContext context) => _context = context;

    public async Task<(List<Post> Posts, int TotalCount)> GetPagedAsync(
        int page, int pageSize, int? categoryId = null, Guid? authorId = null, Guid? viewerUserId = null)
    {
        var query = _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.Comments)
            .AsQueryable();

        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId.Value);
        if (authorId.HasValue) query = query.Where(p => p.AuthorId == authorId.Value);

        var total = await query.CountAsync();
        var posts = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (posts, total);
    }

    public async Task<Post?> GetByIdAsync(int id) =>
        await _context.Posts.FindAsync(id);

    public async Task<Post?> GetByIdWithDetailsAsync(int id) =>
        await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.Comments).ThenInclude(c => c.Author)
            .Include(p => p.Comments).ThenInclude(c => c.Reactions)
            .Include(p => p.Members).ThenInclude(m => m.User)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<int> CountByAuthorAsync(Guid authorId) =>
        await _context.Posts.CountAsync(p => p.AuthorId == authorId);

    public async Task AddAsync(Post post) =>
        await _context.Posts.AddAsync(post);

    public async Task DeleteAsync(Post post) =>
        _context.Posts.Remove(post);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
