using NexusForum.Api.Application.DTOs.Comments;
using NexusForum.Api.Application.DTOs.Posts;
using NexusForum.Api.Application.Interfaces.Services;
using NexusForum.Api.Common.Pagination;
using NexusForum.Api.Common.Results;
using NexusForum.Api.Domain.Entities;
using NexusForum.Api.Domain.Interfaces.Repositories;

namespace NexusForum.Api.Application.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepo;
    private readonly ICategoryRepository _categoryRepo;

    public PostService(IPostRepository postRepo, ICategoryRepository categoryRepo)
    {
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
    }

    public async Task<PagedResult<PostSummaryDto>> GetPagedAsync(int page, int pageSize, int? categoryId = null, Guid? viewerUserId = null)
    {
        var (posts, total) = await _postRepo.GetPagedAsync(page, pageSize, categoryId);
        var items = posts.Select(p => new PostSummaryDto(
            p.Id, p.Title, p.Author.Username, p.Category.Name, p.IsPrivate, p.Comments.Count, p.CreatedAt))
            .ToList();
        return new PagedResult<PostSummaryDto>(items, total, page, pageSize);
    }

    public async Task<Result<PostDto>> GetByIdAsync(int id, Guid? viewerUserId = null)
    {
        var post = await _postRepo.GetByIdWithDetailsAsync(id);
        if (post is null) return Result<PostDto>.Failure("Post not found.", 404);

        if (post.IsPrivate)
        {
            var isAuthor = viewerUserId.HasValue && post.AuthorId == viewerUserId.Value;
            var isMember = viewerUserId.HasValue && post.Members.Any(m => m.UserId == viewerUserId.Value);
            if (!isAuthor && !isMember)
                return Result<PostDto>.Success(ToRestrictedDto(post));
        }

        return Result<PostDto>.Success(ToDto(post));
    }

    public async Task<Result<PostDto>> CreateAsync(CreatePostRequest request, Guid authorId)
    {
        if (await _categoryRepo.GetByIdAsync(request.CategoryId) is null)
            return Result<PostDto>.Failure("Category not found.", 404);

        var post = new Post
        {
            Title = request.Title,
            Content = request.Content,
            AuthorId = authorId,
            CategoryId = request.CategoryId,
            IsPrivate = request.IsPrivate,
            CreatedAt = DateTime.UtcNow
        };

        await _postRepo.AddAsync(post);
        await _postRepo.SaveChangesAsync();

        var created = await _postRepo.GetByIdWithDetailsAsync(post.Id);
        return Result<PostDto>.Success(ToDto(created!), 201);
    }

    public async Task<Result<PostDto>> UpdateAsync(int id, UpdatePostRequest request, Guid requesterId, bool isAdmin)
    {
        var post = await _postRepo.GetByIdWithDetailsAsync(id);
        if (post is null) return Result<PostDto>.Failure("Post not found.", 404);
        if (post.AuthorId != requesterId && !isAdmin) return Result<PostDto>.Failure("Forbidden.", 403);

        post.Title = request.Title;
        post.Content = request.Content;
        post.UpdatedAt = DateTime.UtcNow;

        await _postRepo.SaveChangesAsync();
        return Result<PostDto>.Success(ToDto(post));
    }

    public async Task<Result<bool>> DeleteAsync(int id, Guid requesterId, bool isAdmin)
    {
        var post = await _postRepo.GetByIdAsync(id);
        if (post is null) return Result<bool>.Failure("Post not found.", 404);
        if (post.AuthorId != requesterId && !isAdmin) return Result<bool>.Failure("Forbidden.", 403);

        await _postRepo.DeleteAsync(post);
        await _postRepo.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    private static PostDto ToDto(Post p) => new(
        p.Id, p.Title, p.Content, p.AuthorId, p.Author.Username,
        p.CategoryId, p.Category.Name, p.IsPrivate, IsRestricted: false,
        p.Comments.Select(c => new CommentDto(c.Id, c.Content, c.AuthorId, c.Author.Username,
            c.Reactions.Count(r => r.ReactionType == "up"),
            c.Reactions.Count(r => r.ReactionType == "down"),
            c.CreatedAt, c.UpdatedAt)).ToList(),
        p.Members.Select(m => new PostMemberDto(m.UserId, m.User.Username, m.InvitedAt)).ToList(),
        p.CreatedAt, p.UpdatedAt);

    private static PostDto ToRestrictedDto(Post p) => new(
        p.Id, p.Title, Content: string.Empty, p.AuthorId, p.Author.Username,
        p.CategoryId, p.Category.Name, IsPrivate: true, IsRestricted: true,
        Comments: [], Members: [],
        p.CreatedAt, p.UpdatedAt);
}
