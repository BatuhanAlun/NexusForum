using System.Security.Claims;
using NexusForum.Api.Application.DTOs.Posts;
using NexusForum.Api.Application.Interfaces.Services;

namespace NexusForum.Api.Endpoints;

public static class PrivateThreadEndpoints
{
    public static IEndpointRouteBuilder MapPrivateThreadEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/posts").WithTags("Private Threads");

        // List members — author only
        group.MapGet("/{id:int}/members", async (
            int id,
            IPrivateThreadService service,
            ClaimsPrincipal user) =>
        {
            var requesterId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await service.GetMembersAsync(id, requesterId);
            return result.IsSuccess
                ? Results.Ok(result.Data)
                : Results.Problem(result.Error, statusCode: result.StatusCode);
        })
        .RequireAuthorization()
        .WithName("GetThreadMembers");

        // Invite a user by username — author only
        group.MapPost("/{id:int}/members", async (
            int id,
            InviteUserRequest request,
            IPrivateThreadService service,
            ClaimsPrincipal user) =>
        {
            var requesterId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await service.InviteAsync(id, requesterId, request.Username);
            return result.IsSuccess
                ? Results.Created($"/api/posts/{id}/members/{result.Data!.UserId}", result.Data)
                : Results.Problem(result.Error, statusCode: result.StatusCode);
        })
        .RequireAuthorization()
        .WithName("InviteThreadMember");

        // Remove a member — author only
        group.MapDelete("/{id:int}/members/{userId:guid}", async (
            int id,
            Guid userId,
            IPrivateThreadService service,
            ClaimsPrincipal user) =>
        {
            var requesterId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await service.RemoveMemberAsync(id, requesterId, userId);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(result.Error, statusCode: result.StatusCode);
        })
        .RequireAuthorization()
        .WithName("RemoveThreadMember");

        return app;
    }
}
