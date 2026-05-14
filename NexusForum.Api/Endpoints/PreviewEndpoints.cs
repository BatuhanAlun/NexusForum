namespace NexusForum.Api.Endpoints;

public static class PreviewEndpoints
{
    public record PreviewRequest(string Url);

    public static IEndpointRouteBuilder MapPreviewEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/posts").WithTags("Preview");

        group.MapPost("/preview-link", async (PreviewRequest request) =>
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var response = await http.GetAsync(request.Url);
            var body = await response.Content.ReadAsStringAsync();
            var snippet = body.Length > 2000 ? body[..2000] : body;

            return Results.Ok(new
            {
                statusCode = (int)response.StatusCode,
                contentType = response.Content.Headers.ContentType?.ToString(),
                content = snippet
            });
        })
        .RequireAuthorization()
        .WithName("PreviewLink");

        return app;
    }
}
