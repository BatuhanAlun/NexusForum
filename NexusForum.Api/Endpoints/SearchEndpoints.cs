using NexusForum.Api.Domain.Interfaces.Repositories;

namespace NexusForum.Api.Endpoints;

public static class SearchEndpoints
{
    public static IEndpointRouteBuilder MapSearchEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/posts").WithTags("Search");

        group.MapGet("/search", async (string q, IPostRepository repo) =>
        {
            var posts = await repo.SearchRawAsync(q);
            return Results.Ok(posts.Select(p => new
            {
                p.Id,
                p.Title,
                p.Content,
                p.AuthorId,
                p.CategoryId,
                p.CreatedAt
            }));
        })
        .AllowAnonymous()
        .WithName("SearchPosts");

        return app;
    }
}
