using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Modules.Identity.Entities;
using Shared.Core;

namespace Modules.Identity.Features.Profile;

internal sealed record UpdateProfileCommand(string FirstName, string LastName) : ICommand<Result<UserProfileResponse>>;

internal sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100).WithMessage("First name is required and must be at most 100 characters.");
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100).WithMessage("Last name is required and must be at most 100 characters.");
    }
}

internal sealed class UpdateProfileCommandHandler(
    IUser currentUser,
    UserManager<ApplicationUser> userManager)
    : ICommandHandler<UpdateProfileCommand, Result<UserProfileResponse>>
{
    public async Task<Result<UserProfileResponse>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.Id is null)
            return UserProfileError.InvalidUserId;

        var user = await userManager.FindByIdAsync(currentUser.Id);
        if (user is null)
            return UserProfileError.InvalidUserId;

        user.UpdateProfile(request.FirstName, request.LastName);

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return Error.Failure("Profile.UpdateFailed", errors);
        }

        var roles = await userManager.GetRolesAsync(user);
        return new UserProfileResponse(user.Id, user.FirstName, user.LastName, user.Email, roles);
    }
}
