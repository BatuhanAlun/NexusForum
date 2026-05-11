using NexusForum.Api.Application.DTOs.Comments;
using NexusForum.Api.Application.Interfaces.Services;
using NexusForum.Api.Common.Results;
using NexusForum.Api.Domain.Entities;
using NexusForum.Api.Domain.Interfaces.Repositories;

namespace NexusForum.Api.Application.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepo;
    private readonly IPostRepository _postRepo;

    public CommentService(ICommentRepository commentRepo, IPostRepository postRepo)
    {
        _commentRepo = commentRepo;
        _postRepo = postRepo;
    }

    public async Task<Result<CommentDto>> CreateAsync(int postId, CreateCommentRequest request, Guid authorId)
    {
        if (await _postRepo.GetByIdAsync(postId) is null)
            return Result<CommentDto>.Failure("Post not found.", 404);

        var comment = new Comment
        {
            Content = request.Content,
            AuthorId = authorId,
            PostId = postId,
            CreatedAt = DateTime.UtcNow
        };

        await _commentRepo.AddAsync(comment);
        await _commentRepo.SaveChangesAsync();

        // Reload to get Author navigation populated.
        var saved = await _commentRepo.GetByIdAsync(comment.Id);
        return Result<CommentDto>.Success(
            new CommentDto(saved!.Id, saved.Content, saved.AuthorId, saved.Author.Username, 0, 0, saved.CreatedAt, saved.UpdatedAt), 201);
    }

    public async Task<Result<CommentDto>> UpdateAsync(int id, UpdateCommentRequest request, Guid requesterId, bool isAdmin)
    {
        var comment = await _commentRepo.GetByIdAsync(id);
        if (comment is null) return Result<CommentDto>.Failure("Comment not found.", 404);
        if (comment.AuthorId != requesterId && !isAdmin) return Result<CommentDto>.Failure("Forbidden.", 403);

        comment.Content = request.Content;
        comment.UpdatedAt = DateTime.UtcNow;

        await _commentRepo.SaveChangesAsync();
        return Result<CommentDto>.Success(
            new CommentDto(comment.Id, comment.Content, comment.AuthorId, comment.Author.Username, 0, 0, comment.CreatedAt, comment.UpdatedAt));
    }

    public async Task<Result<bool>> DeleteAsync(int id, Guid requesterId, bool isAdmin)
    {
        var comment = await _commentRepo.GetByIdAsync(id);
        if (comment is null) return Result<bool>.Failure("Comment not found.", 404);
        if (comment.AuthorId != requesterId && !isAdmin) return Result<bool>.Failure("Forbidden.", 403);

        await _commentRepo.DeleteAsync(comment);
        await _commentRepo.SaveChangesAsync();
        return Result<bool>.Success(true);
    }
}
