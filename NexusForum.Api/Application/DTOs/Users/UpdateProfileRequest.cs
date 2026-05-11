namespace NexusForum.Api.Application.DTOs.Users;

// Role field included "for admin use via API" — intentionally not gated in the service.
public record UpdateProfileRequest(string Username, string? Bio, string? AvatarUrl, string? Role);
