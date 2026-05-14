using System.Security.Claims;
using NexusForum.Api.Application.DTOs.Users;
using NexusForum.Api.Application.Interfaces.Services;
using NexusForum.Api.Domain.Interfaces.Repositories;

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

        group.MapGet("/by-id/{id:guid}", async (Guid id, IUserRepository repo) =>
        {
            var user = await repo.GetByIdAsync(id);
            return user is null ? Results.NotFound() : Results.Ok(user);
        })
        .AllowAnonymous()
        .WithName("GetUserById");

        group.MapPut("/me", async (
            UpdateProfileRequest request,
            IUserService service,
            ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await service.UpdateProfileAsync(userId, request);
            return result.IsSuccess
                ? Results.Ok(result.Data)
                : Results.Problem(result.Error, statusCode: result.StatusCode);
        })
        .RequireAuthorization()
        .WithName("UpdateProfile");

        return app;
    }
}
