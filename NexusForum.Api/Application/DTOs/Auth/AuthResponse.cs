namespace NexusForum.Api.Application.DTOs.Auth;

// Record ensures immutability — response objects should never mutate after construction.
public record AuthResponse(Guid Id, string Username, string Email, string Role, string Token);
