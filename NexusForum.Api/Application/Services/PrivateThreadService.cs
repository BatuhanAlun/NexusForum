using NexusForum.Api.Application.DTOs.Posts;
using NexusForum.Api.Application.Interfaces.Services;
using NexusForum.Api.Common.Results;
using NexusForum.Api.Domain.Entities;
using NexusForum.Api.Domain.Interfaces.Repositories;

namespace NexusForum.Api.Application.Services;

public class PrivateThreadService : IPrivateThreadService
{
    private readonly IPostRepository _postRepo;
    private readonly IPostMemberRepository _memberRepo;
    private readonly IUserRepository _userRepo;

    public PrivateThreadService(IPostRepository postRepo, IPostMemberRepository memberRepo, IUserRepository userRepo)
    {
        _postRepo = postRepo;
        _memberRepo = memberRepo;
        _userRepo = userRepo;
    }

    public async Task<Result<PostMemberDto>> InviteAsync(int postId, Guid requesterId, string username)
    {
        var post = await _postRepo.GetByIdAsync(postId);
        if (post is null) return Result<PostMemberDto>.Failure("Post not found.", 404);
        if (!post.IsPrivate) return Result<PostMemberDto>.Failure("Post is not a private thread.", 400);
        if (post.AuthorId != requesterId) return Result<PostMemberDto>.Failure("Only the author can invite members.", 403);

        var target = await _userRepo.GetByUsernameAsync(username);
        if (target is null) return Result<PostMemberDto>.Failure("User not found.", 404);
        if (target.Id == requesterId) return Result<PostMemberDto>.Failure("Cannot invite yourself.", 400);

        if (await _memberRepo.IsMemberAsync(postId, target.Id))
            return Result<PostMemberDto>.Failure("User is already a member of this thread.", 409);

        var member = new PostMember
        {
            PostId      = postId,
            UserId      = target.Id,
            InvitedById = requesterId,
            InvitedAt   = DateTime.UtcNow,
        };

        await _memberRepo.AddAsync(member);
        await _memberRepo.SaveChangesAsync();

        return Result<PostMemberDto>.Success(new PostMemberDto(target.Id, target.Username, member.InvitedAt), 201);
    }

    public async Task<Result<bool>> RemoveMemberAsync(int postId, Guid requesterId, Guid userId)
    {
        var post = await _postRepo.GetByIdAsync(postId);
        if (post is null) return Result<bool>.Failure("Post not found.", 404);
        if (post.AuthorId != requesterId) return Result<bool>.Failure("Only the author can remove members.", 403);

        var member = await _memberRepo.GetAsync(postId, userId);
        if (member is null) return Result<bool>.Failure("Member not found.", 404);

        await _memberRepo.RemoveAsync(member);
        await _memberRepo.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    public async Task<Result<IReadOnlyList<PostMemberDto>>> GetMembersAsync(int postId, Guid requesterId)
    {
        var post = await _postRepo.GetByIdAsync(postId);
        if (post is null) return Result<IReadOnlyList<PostMemberDto>>.Failure("Post not found.", 404);
        if (post.AuthorId != requesterId) return Result<IReadOnlyList<PostMemberDto>>.Failure("Only the author can view members.", 403);

        var members = await _memberRepo.GetByPostIdAsync(postId);
        var dtos = (IReadOnlyList<PostMemberDto>)members
            .Select(m => new PostMemberDto(m.UserId, m.User.Username, m.InvitedAt))
            .ToList();

        return Result<IReadOnlyList<PostMemberDto>>.Success(dtos);
    }
}
