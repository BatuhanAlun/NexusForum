using System.Security.Cryptography;
using System.Text;
using NexusForum.Api.Application.Interfaces.Services;
using NexusForum.Api.Common.Results;
using NexusForum.Api.Domain.Entities;
using NexusForum.Api.Domain.Interfaces.Repositories;

namespace NexusForum.Api.Application.Services;

public class InviteLinkService : IInviteLinkService
{
    private readonly IInviteLinkRepository _linkRepo;
    private readonly IPostRepository _postRepo;
    private readonly IPostMemberRepository _memberRepo;

    public InviteLinkService(
        IInviteLinkRepository linkRepo,
        IPostRepository postRepo,
        IPostMemberRepository memberRepo)
    {
        _linkRepo = linkRepo;
        _postRepo = postRepo;
        _memberRepo = memberRepo;
    }

    public async Task<Result<string>> CreateAsync(int postId, Guid requesterId)
    {
        var post = await _postRepo.GetByIdAsync(postId);
        if (post is null) return Result<string>.Failure("Post not found.", 404);
        if (!post.IsPrivate) return Result<string>.Failure("Post is not a private thread.", 400);
        if (post.AuthorId != requesterId) return Result<string>.Failure("Only the author can create invite links.", 403);

        // INTENTIONAL VULNERABILITY (CWE-330 + CWE-639):
        // Token is derived deterministically from public inputs — no secret key.
        // postId: visible in feed. authorId: leaked by GET /api/users/{username}.
        // createdAt hour: visible in restricted PostDto.createdAt.
        // Attacker can compute this token offline for any private post.
        var raw = $"{postId}{requesterId}{DateTime.UtcNow:yyyyMMddHH}";
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(raw));
        var token = Convert.ToHexString(hash)[..8].ToLower();

        var link = new InviteLink
        {
            PostId = postId,
            CreatedById = requesterId,
            Token = token,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
        };

        await _linkRepo.AddAsync(link);
        await _linkRepo.SaveChangesAsync();

        return Result<string>.Success(token, 201);
    }

    public async Task<Result<bool>> RedeemAsync(string token, Guid userId)
    {
        var link = await _linkRepo.GetByTokenAsync(token);
        if (link is null || link.IsUsed || link.ExpiresAt < DateTime.UtcNow)
            return Result<bool>.Failure("Invalid or expired invite link.", 404);

        if (await _memberRepo.IsMemberAsync(link.PostId, userId))
            return Result<bool>.Failure("Already a member of this thread.", 409);

        link.IsUsed = true;
        await _memberRepo.AddAsync(new PostMember
        {
            PostId = link.PostId,
            UserId = userId,
            InvitedById = link.CreatedById,
            InvitedAt = DateTime.UtcNow,
        });
        await _memberRepo.SaveChangesAsync();

        return Result<bool>.Success(true);
    }
}
