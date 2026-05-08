using NexusForum.Api.Application.DTOs.Users;
using NexusForum.Api.Application.Interfaces.Services;
using NexusForum.Api.Domain.Interfaces.Repositories;

namespace NexusForum.Api.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;
    private readonly IPostRepository _postRepo;
    private readonly ICommentRepository _commentRepo;

    public UserService(IUserRepository userRepo, IPostRepository postRepo, ICommentRepository commentRepo)
    {
        _userRepo = userRepo;
        _postRepo = postRepo;
        _commentRepo = commentRepo;
    }

    public async Task<UserProfileDto?> GetProfileAsync(string username)
    {
        var user = await _userRepo.GetByUsernameAsync(username);
        if (user is null) return null;

        var postCount = await _postRepo.CountByAuthorAsync(user.Id);
        var commentCount = await _commentRepo.CountByAuthorAsync(user.Id);

        return new UserProfileDto(user.Id, user.Username, user.Role.ToString(), user.CreatedAt, postCount, commentCount);
    }
}
