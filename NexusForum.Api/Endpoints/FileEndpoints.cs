namespace NexusForum.Api.Endpoints;

public static class FileEndpoints
{
    public static IEndpointRouteBuilder MapFileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/files").WithTags("Files");

        group.MapGet("/avatar/{name}", (string name) =>
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");
            var fullPath = Path.Combine(basePath, name);

            if (!File.Exists(fullPath))
                return Results.NotFound();

            var bytes = File.ReadAllBytes(fullPath);
            return Results.File(bytes, "application/octet-stream", name);
        })
        .AllowAnonymous()
        .WithName("GetAvatar");

        return app;
    }
}
