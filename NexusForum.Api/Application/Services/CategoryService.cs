using NexusForum.Api.Application.DTOs.Categories;
using NexusForum.Api.Application.Interfaces.Services;
using NexusForum.Api.Domain.Interfaces.Repositories;

namespace NexusForum.Api.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repo;

    public CategoryService(ICategoryRepository repo) => _repo = repo;

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        var categories = await _repo.GetAllAsync();
        return categories
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description, c.Posts.Count))
            .ToList();
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var c = await _repo.GetByIdAsync(id);
        return c is null ? null : new CategoryDto(c.Id, c.Name, c.Description, c.Posts.Count);
    }
}
