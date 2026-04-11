using Shared.Core;

namespace Modules.Identity.Features.UserManagement;
internal struct UserManagementErrors
{
    internal static Error UserNotFound => Error.NotFound("UserManagement.UserNotFound", "User not found");
    internal static Error InvalidRole => Error.Validation("UserManagement.InvalidRole", "The specified role is not valid");
    internal static Error RoleAssignmentFailed(string details) => Error.Failure("UserManagement.RoleAssignmentFailed", details);
    internal static Error CannotDeleteSelf => Error.Failure("UserManagement.CannotDeleteSelf", "You cannot delete your own account");
}
