using NexusForum.Api.Application.Interfaces.Services;

namespace NexusForum.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        group.MapGet("/{username}", async (string username, IUserService service) =>
        {
            var profile = await service.GetProfileAsync(username);
            return profile is null ? Results.NotFound() : Results.Ok(profile);
        })
        .AllowAnonymous()
        .WithName("GetUserProfile");

        return app;
    }
}
