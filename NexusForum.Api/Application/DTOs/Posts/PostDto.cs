using NexusForum.Api.Application.DTOs.Comments;

namespace NexusForum.Api.Application.DTOs.Posts;

public record PostDto(
    int Id,
    string Title,
    string Content,
    Guid AuthorId,
    string AuthorUsername,
    int CategoryId,
    string CategoryName,
    bool IsPrivate,
    bool IsRestricted,
    IReadOnlyList<CommentDto> Comments,
    IReadOnlyList<PostMemberDto> Members,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
