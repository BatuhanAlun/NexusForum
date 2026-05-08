using FluentValidation;
using NexusForum.Api.Application.DTOs.Posts;

namespace NexusForum.Api.Application.Validators.Posts;

public class CreatePostRequestValidator : AbstractValidator<CreatePostRequest>
{
    public CreatePostRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().Length(3, 200);
        RuleFor(x => x.Content).NotEmpty().MinimumLength(10);
        RuleFor(x => x.CategoryId).GreaterThan(0);
    }
}
