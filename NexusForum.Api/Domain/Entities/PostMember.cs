namespace NexusForum.Api.Domain.Entities;

public class PostMember
{
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid InvitedById { get; set; }
    public User InvitedBy { get; set; } = null!;
    public DateTime InvitedAt { get; set; } = DateTime.UtcNow;
}
