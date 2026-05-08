using System.Security.Claims;
using FluentValidation;
using NexusForum.Api.Application.DTOs.Posts;
using NexusForum.Api.Application.Interfaces.Services;

namespace NexusForum.Api.Endpoints;

public static class PostEndpoints
{
    public static IEndpointRouteBuilder MapPostEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/posts").WithTags("Posts");

        group.MapGet("/", async (
            IPostService service,
            int page = 1,
            int pageSize = 20,
            int? categoryId = null) =>
            Results.Ok(await service.GetPagedAsync(page, pageSize, categoryId)))
            .AllowAnonymous()
            .WithName("GetPosts");

        group.MapGet("/{id:int}", async (int id, HttpContext ctx, IPostService service) =>
        {
            Guid? viewerUserId = null;
            if (ctx.User.Identity?.IsAuthenticated == true)
                viewerUserId = Guid.Parse(ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await service.GetByIdAsync(id, viewerUserId);
            return result.IsSuccess
                ? Results.Ok(result.Data)
                : Results.Problem(result.Error, statusCode: result.StatusCode);
        })
        .AllowAnonymous()
        .WithName("GetPost");

        group.MapPost("/", async (
            CreatePostRequest request,
            IPostService service,
            IValidator<CreatePostRequest> validator,
            ClaimsPrincipal user) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());

            var authorId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await service.CreateAsync(request, authorId);
            return result.IsSuccess
                ? Results.Created($"/api/posts/{result.Data!.Id}", result.Data)
                : Results.Problem(result.Error, statusCode: result.StatusCode);
        })
        .RequireAuthorization()
        .WithName("CreatePost");

        group.MapPut("/{id:int}", async (
            int id,
            UpdatePostRequest request,
            IPostService service,
            IValidator<UpdatePostRequest> validator,
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
        .WithName("UpdatePost");

        group.MapDelete("/{id:int}", async (
            int id,
            IPostService service,
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
        .WithName("DeletePost");

        return app;
    }
}
