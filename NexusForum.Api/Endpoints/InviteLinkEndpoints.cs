using System.Security.Claims;
using NexusForum.Api.Application.Interfaces.Services;

namespace NexusForum.Api.Endpoints;

public static class InviteLinkEndpoints
{
    public static IEndpointRouteBuilder MapInviteLinkEndpoints(this IEndpointRouteBuilder app)
    {
        // Generate invite link — author only
        app.MapPost("/api/posts/{id:int}/invite-link", async (
            int id,
            IInviteLinkService service,
            ClaimsPrincipal user) =>
        {
            var requesterId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await service.CreateAsync(id, requesterId);
            return result.IsSuccess
                ? Results.Ok(new { token = result.Data })
                : Results.Problem(result.Error, statusCode: result.StatusCode);
        })
        .RequireAuthorization()
        .WithTags("Private Threads")
        .WithName("CreateInviteLink");

        // Redeem invite link — any authenticated user
        app.MapPost("/api/invites/{token}/redeem", async (
            string token,
            IInviteLinkService service,
            ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await service.RedeemAsync(token, userId);
            return result.IsSuccess
                ? Results.Ok(new { message = "Access granted." })
                : Results.Problem(result.Error, statusCode: result.StatusCode);
        })
        .RequireAuthorization()
        .RequireRateLimiting("invite")
        .WithTags("Private Threads")
        .WithName("RedeemInviteLink");

        return app;
    }
}
