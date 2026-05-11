using NexusForum.Api.Application.DTOs.Users;
using NexusForum.Api.Application.Interfaces.Services;
using NexusForum.Api.Common.Results;
using NexusForum.Api.Domain.Enums;
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

        return ToDto(user, postCount, commentCount);
    }

    public async Task<Result<UserProfileDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user is null) return Result<UserProfileDto>.Failure("User not found.", 404);

        if (user.Username != request.Username)
        {
            if (await _userRepo.ExistsByUsernameAsync(request.Username))
                return Result<UserProfileDto>.Failure("Username already taken.", 409);
        }

        user.Username = request.Username;
        user.Bio = request.Bio ?? user.Bio;
        user.AvatarUrl = request.AvatarUrl ?? user.AvatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        // INTENTIONAL VULNERABILITY (CWE-915): No authorization check on Role field.
        // Any authenticated user can set role=Admin and escalate on next login.
        if (request.Role is not null && Enum.TryParse<UserRole>(request.Role, out var role))
            user.Role = role;

        await _userRepo.SaveChangesAsync();

        var postCount = await _postRepo.CountByAuthorAsync(user.Id);
        var commentCount = await _commentRepo.CountByAuthorAsync(user.Id);
        return Result<UserProfileDto>.Success(ToDto(user, postCount, commentCount));
    }

    private static UserProfileDto ToDto(Domain.Entities.User u, int postCount, int commentCount) =>
        new(u.Id, u.Username, u.Role.ToString(), u.Bio, u.AvatarUrl, u.CreatedAt, postCount, commentCount);
}
