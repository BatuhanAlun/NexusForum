using NexusForum.Api.Application.DTOs.Categories;

namespace NexusForum.Api.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(int id);
}
