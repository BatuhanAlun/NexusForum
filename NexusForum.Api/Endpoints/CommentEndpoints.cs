using System.Security.Claims;
using FluentValidation;
using NexusForum.Api.Application.DTOs.Comments;
using NexusForum.Api.Application.Interfaces.Services;

namespace NexusForum.Api.Endpoints;

file record ReactRequest(string ReactionType);

public static class CommentEndpoints
{
    public static IEndpointRouteBuilder MapCommentEndpoints(this IEndpointRouteBuilder app)
    {
        var posts = app.MapGroup("/api/posts").WithTags("Comments");
        var comments = app.MapGroup("/api/comments").WithTags("Comments");

        posts.MapPost("/{postId:int}/comments", async (
            int postId,
            CreateCommentRequest request,
            ICommentService service,
            IValidator<CreateCommentRequest> validator,
            ClaimsPrincipal user) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());

            var authorId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await service.CreateAsync(postId, request, authorId);
            return result.IsSuccess
                ? Results.Created($"/api/posts/{postId}", result.Data)
                : Results.Problem(result.Error, statusCode: result.StatusCode);
        })
        .RequireAuthorization()
        .WithName("CreateComment");

        comments.MapPut("/{id:int}", async (
            int id,
            UpdateCommentRequest request,
            ICommentService service,
            IValidator<UpdateCommentRequest> validator,
            ClaimsPrincipal user) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());

            var requesterId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isAdmin = user.IsInRole("Admin");
            var result = await service.UpdateAsync(id, request, requesterId, isAdmin);
            return result.IsSuccess
                ? Results.Ok(result.Data)
                : Results.Problem(result.Error, statusCode: result.StatusCode);
        })
        .RequireAuthorization()
        .WithName("UpdateComment");

        comments.MapPost("/{id:int}/react", async (
            int id,
            ReactRequest request,
            IReactionService reactionService,
            ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await reactionService.ReactAsync(id, userId, request.ReactionType);
            return result.IsSuccess
                ? Results.Ok(result.Data)
                : Results.Problem(result.Error, statusCode: result.StatusCode);
        })
        .RequireAuthorization()
        .WithName("ReactToComment");

        comments.MapDelete("/{id:int}", async (
            int id,
            ICommentService service,
            ClaimsPrincipal user) =>
        {
            var requesterId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isAdmin = user.IsInRole("Admin");
            var result = await service.DeleteAsync(id, requesterId, isAdmin);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(result.Error, statusCode: result.StatusCode);
        })
        .RequireAuthorization()
        .WithName("DeleteComment");

        return app;
    }
}
