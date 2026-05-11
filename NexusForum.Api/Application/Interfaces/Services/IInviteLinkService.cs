using NexusForum.Api.Common.Results;

namespace NexusForum.Api.Application.Interfaces.Services;

public interface IInviteLinkService
{
    Task<Result<string>> CreateAsync(int postId, Guid requesterId);
    Task<Result<bool>> RedeemAsync(string token, Guid userId);
}
