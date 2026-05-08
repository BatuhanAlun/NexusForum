using Microsoft.EntityFrameworkCore;
using NexusForum.Api.Domain.Entities;

namespace NexusForum.Api.Infrastructure.Data;

// ApplyConfigurationsFromAssembly scans for all IEntityTypeConfiguration<T> so we never forget to register a new entity.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<RevokedToken> RevokedTokens => Set<RevokedToken>();
    public DbSet<PostMember> PostMembers => Set<PostMember>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
