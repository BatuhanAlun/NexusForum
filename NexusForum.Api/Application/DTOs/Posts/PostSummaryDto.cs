namespace NexusForum.Api.Application.DTOs.Posts;

public record PostSummaryDto(
    int Id,
    string Title,
    string AuthorUsername,
    string CategoryName,
    bool IsPrivate,
    int CommentCount,
    DateTime CreatedAt);
