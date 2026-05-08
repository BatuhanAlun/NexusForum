using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusForum.Api.Domain.Entities;

namespace NexusForum.Api.Infrastructure.Data.Configurations;

public class PostMemberConfiguration : IEntityTypeConfiguration<PostMember>
{
    public void Configure(EntityTypeBuilder<PostMember> builder)
    {
        builder.ToTable("PostMembers");
        builder.HasKey(m => new { m.PostId, m.UserId });

        builder.HasOne(m => m.Post)
            .WithMany(p => p.Members)
            .HasForeignKey(m => m.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.InvitedBy)
            .WithMany()
            .HasForeignKey(m => m.InvitedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(m => m.InvitedAt).IsRequired();
    }
}
