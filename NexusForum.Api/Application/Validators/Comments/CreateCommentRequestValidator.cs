using FluentValidation;
using NexusForum.Api.Application.DTOs.Comments;

namespace NexusForum.Api.Application.Validators.Comments;

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.Content).NotEmpty().Length(1, 2000);
    }
}
