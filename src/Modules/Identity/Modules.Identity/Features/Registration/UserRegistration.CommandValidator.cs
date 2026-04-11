using FluentValidation;
using Modules.Identity.Features.Registration.Services;

namespace Modules.Identity.Features.Registration;
internal sealed class UserRegistrationCommandValidator : AbstractValidator<UserRegistrationCommand>
{
    private readonly IUserRegistrationService _userRegistrationService;
    public UserRegistrationCommandValidator(IUserRegistrationService userRegistrationService)
    {
        _userRegistrationService = userRegistrationService;

        RuleFor(x => x.FullName)
            .NotNull()
            .NotEmpty()
            .WithMessage("Full name can not be empty")
            .MaximumLength(200)
            .WithMessage("Full name must not exceed 200 characters");

        RuleFor(x => x.Email)
            .NotNull()
            .NotEmpty()
            .WithMessage("Email can not be empty")
            .EmailAddress()
            .WithMessage("Invalid email address")
            .MustAsync(async (email, _) => !await IsUserAlreadyExistsWithTheSameEmail(email));

        RuleFor(x => x.Password)
            .NotNull()
            .NotEmpty()
            .WithMessage("Password is required");
    }

    private async Task<bool> IsUserAlreadyExistsWithTheSameEmail(string email)
    {
        return await _userRegistrationService.GetUserDetailsByEmail(email) is not null;
    }
}
