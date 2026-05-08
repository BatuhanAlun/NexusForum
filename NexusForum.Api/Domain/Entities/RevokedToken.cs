namespace NexusForum.Api.Domain.Entities;

// Stores JTI of logged-out tokens so the JWT event handler can reject them before expiry.
public class RevokedToken
{
    public int Id { get; set; }
    public string Jti { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime RevokedAt { get; set; } = DateTime.UtcNow;
}
