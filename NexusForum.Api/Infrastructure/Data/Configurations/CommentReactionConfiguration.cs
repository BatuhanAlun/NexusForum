using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusForum.Api.Domain.Entities;

namespace NexusForum.Api.Infrastructure.Data.Configurations;

public class CommentReactionConfiguration : IEntityTypeConfiguration<CommentReaction>
{
    public void Configure(EntityTypeBuilder<CommentReaction> builder)
    {
        builder.ToTable("CommentReactions");
        builder.HasKey(r => r.Id);

        // INTENTIONAL VULNERABILITY (CWE-362): No unique index on (CommentId, UserId).
        // The application-layer duplicate check is vulnerable to TOCTOU race conditions.
        // A unique constraint here would prevent concurrent inserts from bypassing the check.
        // builder.HasIndex(r => new { r.CommentId, r.UserId }).IsUnique(); // <- intentionally omitted

        builder.Property(r => r.ReactionType)
            .IsRequired()
            .HasMaxLength(4);

        builder.HasOne(r => r.Comment)
            .WithMany(c => c.Reactions)
            .HasForeignKey(r => r.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(r => r.CreatedAt).IsRequired();
    }
}
