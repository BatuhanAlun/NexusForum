using Bogus;
using Microsoft.EntityFrameworkCore;
using NexusForum.Api.Domain.Entities;
using NexusForum.Api.Domain.Enums;

namespace NexusForum.Api.Infrastructure.Data;

// Dev-only seeder using Bogus. Runs once when no users exist.
// Fixed accounts: admin@nexusforum.dev / Admin@1234  |  everyone else: Password1!
public static class DevSeedData
{
    private const int    Seed           = 42;
    private const int    UserCount      = 12;
    private const int    PostCount      = 60;
    private const int    MaxComments    = 10;

    private static readonly string MemberHash =
        BCrypt.Net.BCrypt.HashPassword("Password1!", workFactor: 4);
    private static readonly string AdminHash =
        BCrypt.Net.BCrypt.HashPassword("Admin@1234", workFactor: 4);

    // Tech terms used to build realistic-looking titles
    private static readonly string[] Technologies =
    [
        "Docker", "Kubernetes", "PostgreSQL", "Redis", "React", "Angular",
        "TypeScript", "C#", ".NET", "Git", "REST API", "gRPC", "JWT",
        "OAuth2", "WebSockets", "GraphQL", "Linux", "Nginx", "CI/CD",
        "Entity Framework", "Microservices", "Terraform", "GitHub Actions",
    ];

    private static readonly string[] Verbs =
    [
        "optimize", "debug", "configure", "deploy", "scale", "secure",
        "migrate", "integrate", "refactor", "monitor", "benchmark",
    ];

    private static readonly string[] Environments =
    [
        "production", "Docker", "Kubernetes", "a monolith", "serverless",
        "a microservices setup", "CI/CD pipelines",
    ];

    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        Randomizer.Seed = new Random(Seed);
        var faker = new Faker();

        var categories = await context.Categories.ToListAsync();

        // ── Users ──────────────────────────────────────────────────────────
        var users = BuildUsers(faker);
        context.Users.AddRange(users);

        // ── Posts + Comments ───────────────────────────────────────────────
        var posts = BuildPosts(faker, users, categories);
        context.Posts.AddRange(posts);

        await context.SaveChangesAsync();
    }

    // ── Builders ───────────────────────────────────────────────────────────

    private static List<User> BuildUsers(Faker faker)
    {
        var userFaker = new Faker<User>()
            .RuleFor(u => u.Id,           _  => Guid.NewGuid())
            .RuleFor(u => u.Username,     f  => $"{f.Name.FirstName().ToLower()}_{f.Random.Int(10, 99)}")
            .RuleFor(u => u.Email,        (f, u) => $"{u.Username}@{f.Internet.DomainName()}")
            .RuleFor(u => u.PasswordHash, _  => MemberHash)
            .RuleFor(u => u.Role,         _  => UserRole.Member)
            .RuleFor(u => u.IsActive,     _  => true)
            .RuleFor(u => u.CreatedAt,    f  => f.Date.Past(1).ToUniversalTime());

        var users = userFaker.Generate(UserCount - 1);

        // One guaranteed moderator
        users[0].Role = UserRole.Moderator;

        // Fixed admin so you always have a known login
        users.Insert(0, new User
        {
            Id           = Guid.NewGuid(),
            Username     = "admin",
            Email        = "admin@nexusforum.dev",
            PasswordHash = AdminHash,
            Role         = UserRole.Admin,
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow.AddMonths(-6).ToUniversalTime(),
        });

        return users;
    }

    private static List<Post> BuildPosts(Faker faker, List<User> users, List<Category> categories)
    {
        var posts = new List<Post>();

        for (var i = 0; i < PostCount; i++)
        {
            var createdAt    = faker.Date.Past(1).ToUniversalTime();
            var commentCount = faker.Random.Int(0, MaxComments);

            posts.Add(new Post
            {
                Title      = BuildTitle(faker),
                Content    = BuildContent(faker),
                AuthorId   = faker.PickRandom(users).Id,
                CategoryId = faker.PickRandom(categories).Id,
                CreatedAt  = createdAt,
                Comments   = BuildComments(faker, users, createdAt, commentCount),
            });
        }

        return posts;
    }

    private static List<Comment> BuildComments(
        Faker faker, List<User> users, DateTime postCreatedAt, int count)
    {
        var comments = new List<Comment>();
        var timestamp = postCreatedAt;

        for (var i = 0; i < count; i++)
        {
            timestamp = timestamp.AddMinutes(faker.Random.Int(5, 720));

            comments.Add(new Comment
            {
                Content   = faker.Lorem.Sentences(faker.Random.Int(1, 4)),
                AuthorId  = faker.PickRandom(users).Id,
                CreatedAt = timestamp > DateTime.UtcNow ? DateTime.UtcNow : timestamp,
            });
        }

        return comments;
    }

    // ── Content helpers ────────────────────────────────────────────────────

    private static string BuildTitle(Faker f)
    {
        var tech  = f.PickRandom(Technologies);
        var tech2 = f.PickRandom(Technologies);
        var verb  = f.PickRandom(Verbs);
        var env   = f.PickRandom(Environments);

        return f.Random.Int(0, 5) switch
        {
            0 => $"How do I {verb} {tech} in {env}?",
            1 => $"{tech} vs {tech2} — which should I choose?",
            2 => $"Best practices for {verb}ing {tech}",
            3 => $"Getting started with {tech}: a practical guide",
            4 => $"Why is my {tech} {verb}ation failing in {env}?",
            _ => $"[Question] {tech} {verb}ation — am I doing this right?",
        };
    }

    private static string BuildContent(Faker f)
    {
        var paragraphs = f.Random.Int(1, 5);
        var lines = new List<string>();

        for (var i = 0; i < paragraphs; i++)
            lines.Add(f.Lorem.Paragraph(f.Random.Int(2, 6)));

        // Occasionally add a code block for realism
        if (f.Random.Bool(0.35f))
        {
            var snippets = new[]
            {
                "var result = await service.GetAsync(id);\nif (result is null) return NotFound();",
                "SELECT * FROM posts WHERE category_id = $1 ORDER BY created_at DESC LIMIT 20;",
                "docker compose up --build -d\ndocker compose logs -f api",
                "git rebase -i HEAD~3\n# squash commits before pushing",
                "const data = await fetch('/api/posts').then(r => r.json());",
            };

            lines.Insert(f.Random.Int(0, lines.Count),
                $"```\n{f.PickRandom(snippets)}\n```");
        }

        return string.Join("\n\n", lines);
    }
}
