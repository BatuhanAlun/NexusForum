namespace NexusForum.Api.Endpoints;

public static class FileEndpoints
{
    public static IEndpointRouteBuilder MapFileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/files").WithTags("Files");

        // INTENTIONAL VULNERABILITY (CWE-22, Path Traversal):
        // Catch-all route allows `/` in the param. `Path.Combine` does not normalize
        // `..` sequences, and File.ReadAllBytes follows them at the OS level.
        // Realistic exploit: GET /api/files/avatar/../../appsettings.Development.json
        // → leaks JWT signing key from the dev config baked into the container.
        group.MapGet("/avatar/{**name}", (string name) =>
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");
            var fullPath = Path.Combine(basePath, name);

            if (!File.Exists(fullPath))
                return Results.NotFound();

            var bytes = File.ReadAllBytes(fullPath);
            return Results.File(bytes, "application/octet-stream", Path.GetFileName(name));
        })
        .AllowAnonymous()
        .WithName("GetAvatar");

        return app;
    }
}
