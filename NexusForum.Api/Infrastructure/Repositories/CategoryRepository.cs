using Microsoft.EntityFrameworkCore;
using NexusForum.Api.Domain.Entities;
using NexusForum.Api.Domain.Interfaces.Repositories;
using NexusForum.Api.Infrastructure.Data;

namespace NexusForum.Api.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context) => _context = context;

    public async Task<List<Category>> GetAllAsync() =>
        await _context.Categories.Include(c => c.Posts).ToListAsync();

    public async Task<Category?> GetByIdAsync(int id) =>
        await _context.Categories.Include(c => c.Posts).FirstOrDefaultAsync(c => c.Id == id);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
