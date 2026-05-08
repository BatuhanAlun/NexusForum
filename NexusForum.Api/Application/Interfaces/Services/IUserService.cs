using NexusForum.Api.Application.DTOs.Users;

namespace NexusForum.Api.Application.Interfaces.Services;

public interface IUserService
{
    Task<UserProfileDto?> GetProfileAsync(string username);
}
