using Newtonsoft.Json;

namespace NexusForum.Api.Endpoints;

public static class ImportEndpoints
{
    public static IEndpointRouteBuilder MapImportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/posts").WithTags("Import");

        group.MapPost("/import", async (HttpRequest req) =>
        {
            using var reader = new StreamReader(req.Body);
            var raw = await reader.ReadToEndAsync();

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            var obj = JsonConvert.DeserializeObject<object>(raw, settings);
            return Results.Ok(new { imported = obj?.GetType().FullName });
        })
        .RequireAuthorization()
        .Accepts<object>("application/json")
        .WithName("ImportPost");

        return app;
    }
}
