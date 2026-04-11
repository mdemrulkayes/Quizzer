using Shared.Core;

namespace Modules.Identity.Features.ChangePassword;
internal struct ChangePasswordErrors
{
    internal static Error InvalidUserId => Error.Unauthorized("ChangePassword.InvalidUserId", "Invalid user information");
    internal static Error UserNotFound => Error.NotFound("ChangePassword.UserNotFound", "User not found");
    internal static Error PasswordChangeFailed(string details) => Error.Failure("ChangePassword.Failed", details);
}
