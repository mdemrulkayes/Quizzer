using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Entities;
using Shared.Core;

namespace Modules.Identity.Features.UserManagement;

internal sealed class GetAllUsersQueryHandler(
    UserManager<ApplicationUser> userManager)
    : IQueryHandler<GetAllUsersQuery, Result<UserListResponse>>
{
    public async Task<Result<UserListResponse>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var query = userManager.Users.OrderBy(u => u.FirstName).AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var userResponses = new List<UserListItemResponse>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            userResponses.Add(new UserListItemResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                roles,
                user.IsDeleted,
                user.CreatedDate,
                user.LastLoginTime));
        }

        return new UserListResponse(totalCount, userResponses);
    }
}

internal sealed class GetUserByIdQueryHandler(
    UserManager<ApplicationUser> userManager)
    : IQueryHandler<GetUserByIdQuery, Result<UserDetailResponse>>
{
    public async Task<Result<UserDetailResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            return UserManagementErrors.UserNotFound;
        }

        var roles = await userManager.GetRolesAsync(user);

        return new UserDetailResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PhoneNumber,
            roles,
            user.IsDeleted,
            user.CreatedDate,
            user.UpdatedDate,
            user.LastLoginTime);
    }
}
