using Microsoft.EntityFrameworkCore;
using NexusForum.Api.Infrastructure.Data;

namespace NexusForum.Api.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin").WithTags("Admin");

        // Export all user records — intentionally returns raw User entity including PasswordHash.
        group.MapGet("/export", async (AppDbContext db) =>
        {
            var users = await db.Users.ToListAsync();
            return Results.Ok(users);
        })
        .RequireAuthorization(p => p.RequireRole("Admin"))
        .WithName("ExportUsers");

        return app;
    }
}
