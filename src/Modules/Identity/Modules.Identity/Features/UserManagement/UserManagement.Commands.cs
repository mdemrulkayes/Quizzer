using Shared.Core;

namespace Modules.Identity.Features.UserManagement;

internal sealed record UpdateUserRoleCommand(Guid UserId, string RoleName) : ICommand<Result<bool>>;

internal sealed record DeleteUserCommand(Guid UserId) : ICommand<Result<bool>>;
