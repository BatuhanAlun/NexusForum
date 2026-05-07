using System.Security.Claims;
using FluentValidation;
using NexusForum.Api.Application.DTOs.Auth;
using NexusForum.Api.Application.Interfaces.Services;

namespace NexusForum.Api.Endpoints;

// Extension method keeps Program.cs clean — each feature group owns its own endpoint registration.
public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (
            RegisterRequest request,
            IAuthService authService,
            IValidator<RegisterRequest> validator) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var result = await authService.RegisterAsync(request);
            return result.IsSuccess
                ? Results.Created($"/api/auth/me", result.Data)
                : Results.Problem(result.Error, statusCode: result.StatusCode);
        })
        .AllowAnonymous()
        .WithName("Register")
        .Produces<AuthResponse>(201)
        .ProducesValidationProblem()
        .ProducesProblem(409);

        group.MapPost("/login", async (
            LoginRequest request,
            IAuthService authService,
            IValidator<LoginRequest> validator) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var result = await authService.LoginAsync(request);
            return result.IsSuccess
                ? Results.Ok(result.Data)
                : Results.Problem(result.Error, statusCode: result.StatusCode);
        })
        .AllowAnonymous()
        .WithName("Login")
        .Produces<AuthResponse>()
        .ProducesValidationProblem()
        .ProducesProblem(401);


        // Protected endpoint — proves the JWT is valid and returns the caller's identity.
        group.MapGet("/me", (ClaimsPrincipal user) => Results.Ok(new
        {
            Id = user.FindFirstValue(ClaimTypes.NameIdentifier),
            Username = user.FindFirstValue(ClaimTypes.Name),
            Email = user.FindFirstValue(ClaimTypes.Email),
            Role = user.FindFirstValue(ClaimTypes.Role)
        }))
        .RequireAuthorization()
        .WithName("Me")
        .Produces(200)
        .ProducesProblem(401);

        return app;
    }
}
