using FluentValidation;

namespace Modules.Identity.Features.ChangePassword;
internal sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotNull()
            .NotEmpty()
            .WithMessage("Current password is required");

        RuleFor(x => x.NewPassword)
            .NotNull()
            .NotEmpty()
            .WithMessage("New password is required")
            .MinimumLength(6)
            .WithMessage("New password must be at least 6 characters");

        RuleFor(x => x.ConfirmNewPassword)
            .NotNull()
            .NotEmpty()
            .WithMessage("Confirm new password is required")
            .Equal(x => x.NewPassword)
            .WithMessage("New password and confirmation do not match");
    }
}
