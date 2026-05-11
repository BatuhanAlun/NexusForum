using NexusForum.Api.Application.DTOs.Users;
using NexusForum.Api.Common.Results;

namespace NexusForum.Api.Application.Interfaces.Services;

public interface IUserService
{
    Task<UserProfileDto?> GetProfileAsync(string username);
    Task<Result<UserProfileDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
}
