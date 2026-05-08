using Microsoft.EntityFrameworkCore;
using NexusForum.Api.Domain.Entities;

namespace NexusForum.Api.Infrastructure.Data;

// Runs once on startup — idempotent (skips if categories already exist).
public static class SeedData
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Categories.AnyAsync()) return;

        context.Categories.AddRange(
            new Category { Name = "General", Description = "General discussion" },
            new Category { Name = "Technology", Description = "Tech, code, tools" },
            new Category { Name = "Security", Description = "InfoSec, vulnerabilities, CTFs" },
            new Category { Name = "Off Topic", Description = "Anything goes" }
        );

        await context.SaveChangesAsync();
    }
}
