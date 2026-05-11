namespace NexusForum.Api.Application.DTOs.Users;

public record UserProfileDto(
    Guid Id,
    string Username,
    string Role,
    string Bio,
    string? AvatarUrl,
    DateTime CreatedAt,
    int PostCount,
    int CommentCount);
