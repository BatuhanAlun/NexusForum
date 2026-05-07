using System.IdentityModel.Tokens.Jwt;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
builder.Services.AddScoped<IAuthService, AuthService>();

// Scans the assembly for all AbstractValidator<T> implementations automatically.
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ── Authentication ──────────────────────────────────────────────────────────
// Disable the legacy claim type map so JWT claim names (sub, email, role) are preserved as-is.
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            // Clock skew default is 5 min — set to zero for strict expiry enforcement.
            ClockSkew = TimeSpan.Zero,
            // Map short "role" claim to ClaimTypes.Role so RequireAuthorization(roles:) works.
            RoleClaimType = "role",
            NameClaimType = JwtRegisteredClaimNames.UniqueName
        };
    });

builder.Services.AddAuthorization();

// ── CORS ────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
        policy.WithOrigins(
                "http://localhost:4200",  // ng serve (dev)
                "http://localhost:80")    // Docker nginx
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ── OpenAPI ─────────────────────────────────────────────────────────────────
// Two transformers: document-level adds the Bearer scheme; operation-level locks protected endpoints.
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddOperationTransformer<SecurityRequirementsOperationTransformer>();
});

// ── Build ───────────────────────────────────────────────────────────────────
var app = builder.Build();

// Apply pending EF migrations on startup so Docker containers self-configure.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // Scalar UI available at /scalar — interactive docs with JWT auth support.
    app.MapScalarApiReference(options => options
        .WithTitle("NexusForum API")
        .WithTheme(ScalarTheme.DeepSpace)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient));
}

app.UseCors("Angular");
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();

app.Run();
