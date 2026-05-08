using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusForum.Api.Domain.Entities;

namespace NexusForum.Api.Infrastructure.Data.Configurations;

public class RevokedTokenConfiguration : IEntityTypeConfiguration<RevokedToken>
{
    public void Configure(EntityTypeBuilder<RevokedToken> builder)
    {
        builder.HasKey(r => r.Id);
        // Index on Jti for fast lookup on every authenticated request.
        builder.HasIndex(r => r.Jti).IsUnique();
        builder.Property(r => r.Jti).IsRequired().HasMaxLength(36);
    }
}
