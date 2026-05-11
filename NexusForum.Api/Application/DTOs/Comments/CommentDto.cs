namespace NexusForum.Api.Application.DTOs.Comments;

public record CommentDto(
    int Id,
    string Content,
    Guid AuthorId,
    string AuthorUsername,
    int UpCount,
    int DownCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
