using NexusForum.Api.Application.DTOs.Auth;
using NexusForum.Api.Common.Results;

namespace NexusForum.Api.Application.Interfaces.Services;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
    Task LogoutAsync(string jti, DateTime tokenExpiry);
}
