using Shared.Core;
using Shared.Core.Caching;

namespace Modules.Identity.Features.UserManagement;

internal sealed record GetAllUsersQuery(int PageNumber = 1, int PageSize = 10)
    : IQuery<Result<UserListResponse>>, ICacheableQuery
{
    public string CacheKey => $"{CacheKeys.Users}:all:{PageNumber}:{PageSize}";
}

internal sealed record GetUserByIdQuery(Guid UserId)
    : IQuery<Result<UserDetailResponse>>, ICacheableQuery
{
    public string CacheKey => $"{CacheKeys.Users}:id:{UserId}";
}
