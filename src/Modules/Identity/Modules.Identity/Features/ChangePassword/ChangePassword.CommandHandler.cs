using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Modules.Identity.Entities;
using Shared.Core;

namespace Modules.Identity.Features.ChangePassword;
internal sealed class ChangePasswordCommandHandler(
    UserManager<ApplicationUser> userManager,
    IUser currentUser,
    ILogger<ChangePasswordCommandHandler> logger) : ICommandHandler<ChangePasswordCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(currentUser.Id))
        {
            return ChangePasswordErrors.InvalidUserId;
        }

        var user = await userManager.FindByIdAsync(currentUser.Id);
        if (user == null)
        {
            return ChangePasswordErrors.UserNotFound;
        }

        var result = await userManager.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            logger.LogWarning("Password change failed for user {UserId}: {Errors}", currentUser.Id, errors);
            return ChangePasswordErrors.PasswordChangeFailed(errors);
        }

        logger.LogInformation("Password changed successfully for user {UserId}", currentUser.Id);
        return true;
    }
}
