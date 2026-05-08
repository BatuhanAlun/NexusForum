namespace NexusForum.Api.Application.DTOs.Posts;

public record PostMemberDto(Guid UserId, string Username, DateTime InvitedAt);
