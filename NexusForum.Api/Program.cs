using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NexusForum.Api.Application.Interfaces.Services;
using NexusForum.Api.Application.Services;
using NexusForum.Api.Domain.Interfaces.Repositories;
using NexusForum.Api.Endpoints;
using NexusForum.Api.Infrastructure.Data;
using NexusForum.Api.Infrastructure.OpenApi;
using NexusForum.Api.Infrastructure.Repositories;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ── Infrastructure ──────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Dependency Injection ────────────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IRevokedTokenRepository, RevokedTokenRepository>();
builder.Services.AddScoped<IPostMemberRepository, PostMemberRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPrivateThreadService, PrivateThreadService>();
builder.Services.AddScoped<IInviteLinkRepository, InviteLinkRepository>();
builder.Services.AddScoped<IInviteLinkService, InviteLinkService>();
builder.Services.AddScoped<ICommentReactionRepository, CommentReactionRepository>();
builder.Services.AddScoped<IReactionService, ReactionService>();
builder.Services.AddSingleton<PentesterRunner>();

// Scans the assembly for all AbstractValidator<T> implementations automatically.
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ── Authentication ──────────────────────────────────────────────────────────
// Disable the legacy claim type map so JWT claim names are preserved as-is.
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // .NET 10 default handler is JsonWebTokenHandler — disable inbound claim remapping
        // so short claim names ("role", "sub", etc.) are preserved as-is.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = "role",
            NameClaimType = JwtRegisteredClaimNames.UniqueName
        };

        // Reject tokens whose JTI appears in the RevokedTokens table (logout support).
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                // SSE EventSource can't set custom headers; accept token from query string
                // for /api/pentester/stream/* only.
                if (string.IsNullOrEmpty(ctx.Token) &&
                    ctx.Request.Path.StartsWithSegments("/api/pentester/stream"))
                {
                    var qs = ctx.Request.Query["access_token"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(qs)) ctx.Token = qs;
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = async context =>
            {
                var jti = context.Principal?.FindFirst("jti")?.Value;
                if (jti is null) return;

                var repo = context.HttpContext.RequestServices
                    .GetRequiredService<IRevokedTokenRepository>();

                if (await repo.IsRevokedAsync(jti))
                    context.Fail("Token has been revoked.");
            }
        };
    });

builder.Services.AddAuthorization();

// ── Rate Limiting ────────────────────────────────────────────────────────────
// INTENTIONAL VULNERABILITY (CWE-290 + CWE-799):
// Partition key trusts the client-supplied X-Forwarded-For header without validation.
// Attacker rotates this header per request to bypass the limit entirely.
builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        context.HttpContext.Response.Headers.RetryAfter = "60";
        await context.HttpContext.Response.WriteAsync("Too many requests. Try again later.", token);
    };

    options.AddPolicy("auth", context =>
    {
        // FLAW: reads attacker-controlled header as the rate-limit key
        var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                 ?? context.Connection.RemoteIpAddress?.ToString()
                 ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,
        });
    });

    options.AddPolicy("invite", context =>
    {
        // Same flaw — invite redemption rate limit also bypassable
        var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                 ?? context.Connection.RemoteIpAddress?.ToString()
                 ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter("invite_" + ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,
        });
    });
});

// ── CORS ────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
        policy.WithOrigins(
                "http://localhost:4200",
                "http://localhost:80")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ── OpenAPI ─────────────────────────────────────────────────────────────────
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddOperationTransformer<SecurityRequirementsOperationTransformer>();
});

// ── Build ───────────────────────────────────────────────────────────────────
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.SeedAsync(db);
    if (app.Environment.IsDevelopment())
        await DevSeedData.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options
        .WithTitle("NexusForum API")
        .WithTheme(ScalarTheme.DeepSpace)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient));
}

app.UseCors("Angular");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.MapAuthEndpoints();
app.MapCategoryEndpoints();
app.MapPostEndpoints();
app.MapCommentEndpoints();
app.MapUserEndpoints();
app.MapPrivateThreadEndpoints();
app.MapAdminEndpoints();
app.MapInviteLinkEndpoints();
app.MapSearchEndpoints();
app.MapFileEndpoints();
app.MapPreviewEndpoints();
app.MapImportEndpoints();
app.MapPentesterEndpoints();

app.Run();
