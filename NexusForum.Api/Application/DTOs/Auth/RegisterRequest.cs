namespace NexusForum.Api.Application.DTOs.Auth;

public record RegisterRequest(string Username, string Email, string Password)
{
    public string? Role { get; init; }
}
