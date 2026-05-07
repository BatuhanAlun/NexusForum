using FluentValidation;
using NexusForum.Api.Application.DTOs.Auth;

namespace NexusForum.Api.Application.Validators;

// Validators live in Application because they enforce business input rules, not domain invariants.
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .Length(3, 30)
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("Username may only contain letters, numbers, and underscores.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128);
    }
}
