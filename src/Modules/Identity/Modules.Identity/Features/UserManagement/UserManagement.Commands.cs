using Shared.Core;
using Shared.Core.Caching;

namespace Modules.Identity.Features.UserManagement;

internal sealed record UpdateUserRoleCommand(Guid UserId, string[] RoleNames) : ICommand<Result<bool>>, ICacheInvalidatingCommand
{
    public string[] CacheKeysToInvalidate =>
    [
        $"{CacheKeys.Users}:all:",
        $"{CacheKeys.Users}:id:{UserId}",
    ];
}

internal sealed record DeleteUserCommand(Guid UserId) : ICommand<Result<bool>>, ICacheInvalidatingCommand
{
    public string[] CacheKeysToInvalidate =>
    [
        $"{CacheKeys.Users}:all:",
        $"{CacheKeys.Users}:id:{UserId}",
    ];
}
