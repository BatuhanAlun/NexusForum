namespace NexusForum.Api.Application.DTOs.Comments;

public record ReactionCountDto(int UpCount, int DownCount, string? MyReaction);
