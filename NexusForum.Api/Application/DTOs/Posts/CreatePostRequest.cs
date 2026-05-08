namespace NexusForum.Api.Application.DTOs.Posts;

public record CreatePostRequest(string Title, string Content, int CategoryId, bool IsPrivate = false);
