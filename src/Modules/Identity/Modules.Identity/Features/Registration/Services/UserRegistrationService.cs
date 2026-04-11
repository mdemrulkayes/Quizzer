using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Modules.Identity.Constants;
using Modules.Identity.Entities;
using Modules.Identity.Features.Registration.Enums;
using Modules.Identity.Features.Registration.Events;
using Shared.Core;
using Shared.Core.IntegrationEvents;
using Shared.Core.IntegrationEvents.Events;

namespace Modules.Identity.Features.Registration.Services;
internal class UserRegistrationService(
    UserManager<ApplicationUser> userManager,
    ITimeProvider timeProvider,
    ILogger<UserRegistrationService> logger,
    IMediator mediator,
    IIntegrationEventPublisher eventPublisher
    ) : IUserRegistrationService
{
    public async Task<Result<bool>> RegisterUser(UserRegistrationCommand command)
    {
        var (firstName, lastName) = SplitFullName(command.FullName);

        var user = ApplicationUser.RegisterUser(firstName, lastName, command.Email, null, UserType.Examine, timeProvider);
        if (!user.IsSuccess || user.Value is null)
        {
            return user.Error;
        }
        logger.LogInformation("Application user instance created to register user");
        var result = await userManager.CreateAsync(user.Value, command.Password);
        if (!result.Succeeded)
        {
            return RegistrationErrors.IdentityError(result.Errors);
        }
        await AssignToRole(user.Value);

        await mediator.Publish(new SendWelcomeEmailAfterUserRegistered(user.Value.FirstName, user.Value.LastName,
            user.Value.Email, "Welcome to the Quizzer", timeProvider));

        await eventPublisher.PublishAsync(
            new UserRegisteredEvent(user.Value.Id, user.Value.Email!, user.Value.UserType.ToString()));

        return result.Succeeded;
    }

    private static (string FirstName, string LastName) SplitFullName(string fullName)
    {
        var trimmed = fullName.Trim();
        var spaceIndex = trimmed.IndexOf(' ');
        if (spaceIndex < 0)
            return (trimmed, string.Empty);

        return (trimmed[..spaceIndex], trimmed[(spaceIndex + 1)..].Trim());
    }

    public async Task<ApplicationUser?> GetUserDetailsByEmail(string email)
    {
        return await userManager.FindByEmailAsync(email);
    }

    #region Private methods

    private async Task AssignToRole(ApplicationUser user)
    {
        var roleName = user.UserType switch
        {
            UserType.QuizAuthor => RoleConstants.QuizAuthor,
            UserType.Examine => RoleConstants.Examine,
            _ => ""
        };
        if (!string.IsNullOrWhiteSpace(roleName))
        {
            var roleAssignResult = await userManager.AddToRoleAsync(user, roleName);
            if (!roleAssignResult.Succeeded)
            {
                logger.LogCritical("User created successfully but can not assign to role.");
            }
        }
        else
        {
            logger.LogError("Invalid role to assign a registered user");
        }
    }

    #endregion
}
