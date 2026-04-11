using FluentValidation;

namespace Modules.Identity.Features.RefreshToken;
internal sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.AccessToken)
            .NotNull()
            .NotEmpty()
            .WithMessage("Access token is required");

        RuleFor(x => x.RefreshToken)
            .NotNull()
            .NotEmpty()
            .WithMessage("Refresh token is required");
    }
}
