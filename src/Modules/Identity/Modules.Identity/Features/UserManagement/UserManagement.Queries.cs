using Shared.Core;

namespace Modules.Identity.Features.UserManagement;

internal sealed record GetAllUsersQuery(int PageNumber = 1, int PageSize = 10) : IQuery<Result<UserListResponse>>;

internal sealed record GetUserByIdQuery(Guid UserId) : IQuery<Result<UserDetailResponse>>;
