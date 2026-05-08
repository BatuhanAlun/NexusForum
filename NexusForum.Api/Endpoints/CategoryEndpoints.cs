using NexusForum.Api.Application.Interfaces.Services;

namespace NexusForum.Api.Endpoints;

public static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories").WithTags("Categories");

        group.MapGet("/", async (ICategoryService service) =>
            Results.Ok(await service.GetAllAsync()))
            .AllowAnonymous()
            .WithName("GetCategories");

        group.MapGet("/{id:int}", async (int id, ICategoryService service) =>
        {
            var category = await service.GetByIdAsync(id);
            return category is null ? Results.NotFound() : Results.Ok(category);
        })
        .AllowAnonymous()
        .WithName("GetCategory");

        return app;
    }
}
