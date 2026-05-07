using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusForum.Api.Domain.Entities;
using NexusForum.Api.Domain.Enums;

namespace NexusForum.Api.Infrastructure.Data.Configurations;

// Fluent API configuration is preferred over data annotations — keeps domain entities free of EF concerns.
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(30);

        builder.HasIndex(u => u.Username)
            .IsUnique();

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        // Store enum as string so DB rows are self-documenting.
        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasDefaultValue(UserRole.Member)
            .HasMaxLength(20);

        builder.Property(u => u.CreatedAt)
            .IsRequired();
    }
}
