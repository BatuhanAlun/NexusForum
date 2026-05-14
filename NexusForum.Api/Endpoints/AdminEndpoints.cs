using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using NexusForum.Api.Infrastructure.Data;

namespace NexusForum.Api.Endpoints;

public static class AdminEndpoints
{
    public record PingRequest(string Host);

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

        group.MapPost("/ping", async (PingRequest request) =>
        {
            var psi = new ProcessStartInfo
            {
                FileName = "/bin/sh",
                Arguments = $"-c \"ping -c 1 {request.Host}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var proc = Process.Start(psi)!;
            var stdout = await proc.StandardOutput.ReadToEndAsync();
            var stderr = await proc.StandardError.ReadToEndAsync();
            await proc.WaitForExitAsync();

            return Results.Ok(new { output = stdout, error = stderr, exitCode = proc.ExitCode });
        })
        .RequireAuthorization(p => p.RequireRole("Admin"))
        .WithName("AdminPing");

        return app;
    }
}
