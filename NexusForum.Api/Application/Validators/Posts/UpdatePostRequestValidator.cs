using FluentValidation;
using NexusForum.Api.Application.DTOs.Posts;

namespace NexusForum.Api.Application.Validators.Posts;

public class UpdatePostRequestValidator : AbstractValidator<UpdatePostRequest>
{
    public UpdatePostRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().Length(3, 200);
        RuleFor(x => x.Content).NotEmpty().MinimumLength(10);
    }
}
