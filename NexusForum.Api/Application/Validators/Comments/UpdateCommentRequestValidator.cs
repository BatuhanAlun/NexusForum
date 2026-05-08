using FluentValidation;
using NexusForum.Api.Application.DTOs.Comments;

namespace NexusForum.Api.Application.Validators.Comments;

public class UpdateCommentRequestValidator : AbstractValidator<UpdateCommentRequest>
{
    public UpdateCommentRequestValidator()
    {
        RuleFor(x => x.Content).NotEmpty().Length(1, 2000);
    }
}
