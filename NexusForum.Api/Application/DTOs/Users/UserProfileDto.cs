namespace NexusForum.Api.Application.DTOs.Users;

public record UserProfileDto(
    Guid Id,
    string Username,
    string Role,
    DateTime CreatedAt,
    int PostCount,
    int CommentCount);
