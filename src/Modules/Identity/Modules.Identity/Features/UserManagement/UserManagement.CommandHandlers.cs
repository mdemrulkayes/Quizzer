using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Modules.Identity.Entities;
using Shared.Core;
using Shared.Core.IntegrationEvents;
using Shared.Core.IntegrationEvents.Events;

namespace Modules.Identity.Features.UserManagement;

internal sealed class UpdateUserRoleCommandHandler(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    ILogger<UpdateUserRoleCommandHandler> logger)
    : ICommandHandler<UpdateUserRoleCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateUserRoleCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.UserId.ToString());
        if (user == null)
        {
            return UserManagementErrors.UserNotFound;
        }

        foreach (var roleName in command.RoleNames)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
                return UserManagementErrors.InvalidRole;
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Any())
        {
            var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                var errors = string.Join("; ", removeResult.Errors.Select(e => e.Description));
                return UserManagementErrors.RoleAssignmentFailed(errors);
            }
        }

        var addResult = await userManager.AddToRolesAsync(user, command.RoleNames);
        if (!addResult.Succeeded)
        {
            var errors = string.Join("; ", addResult.Errors.Select(e => e.Description));
            return UserManagementErrors.RoleAssignmentFailed(errors);
        }

        logger.LogInformation("Roles updated to [{Roles}] for user {UserId}", string.Join(", ", command.RoleNames), command.UserId);
        return true;
    }
}

internal sealed class DeleteUserCommandHandler(
    UserManager<ApplicationUser> userManager,
    IUser currentUser,
    ILogger<DeleteUserCommandHandler> logger,
    ITimeProvider timeProvider,
    IIntegrationEventPublisher eventPublisher)
    : ICommandHandler<DeleteUserCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.Id == command.UserId.ToString())
        {
            return UserManagementErrors.CannotDeleteSelf;
        }

        var user = await userManager.FindByIdAsync(command.UserId.ToString());
        if (user == null)
        {
            return UserManagementErrors.UserNotFound;
        }

        user.IsDeleted = true;
        user.UpdatedDate = timeProvider.TimeNow;
        if (!string.IsNullOrEmpty(currentUser.Id))
        {
            user.UpdatedBy = Guid.Parse(currentUser.Id);
        }

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to soft-delete user {UserId}", command.UserId);
            return UserManagementErrors.UserNotFound;
        }

        logger.LogInformation("User {UserId} soft-deleted by {CurrentUserId}", command.UserId, currentUser.Id);

        await eventPublisher.PublishAsync(
            new UserDeletedEvent(command.UserId), cancellationToken);

        return true;
    }
}
