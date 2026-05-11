namespace NexusForum.Api.Domain.Entities;

public class CommentReaction
{
    public int Id { get; set; }
    public int CommentId { get; set; }
    public Comment Comment { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string ReactionType { get; set; } = "up"; // "up" or "down"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
