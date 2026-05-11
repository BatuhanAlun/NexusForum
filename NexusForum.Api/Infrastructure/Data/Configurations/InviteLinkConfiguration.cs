using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusForum.Api.Domain.Entities;

namespace NexusForum.Api.Infrastructure.Data.Configurations;

public class InviteLinkConfiguration : IEntityTypeConfiguration<InviteLink>
{
    public void Configure(EntityTypeBuilder<InviteLink> builder)
    {
        builder.ToTable("InviteLinks");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Token)
            .IsRequired()
            .HasMaxLength(16);

        builder.HasOne(l => l.Post)
            .WithMany()
            .HasForeignKey(l => l.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.CreatedBy)
            .WithMany()
            .HasForeignKey(l => l.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(l => l.CreatedAt).IsRequired();
        builder.Property(l => l.ExpiresAt).IsRequired();
    }
}
