using MediatR;
using Microsoft.Extensions.Logging;

namespace Modules.Identity.Features.Registration.Events;

internal sealed class SendWelcomeEmailAfterUserRegisteredHandler(
    ILogger<SendWelcomeEmailAfterUserRegisteredHandler> logger)
    : INotificationHandler<SendWelcomeEmailAfterUserRegistered>
{
    public Task Handle(SendWelcomeEmailAfterUserRegistered notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Welcome email event received for {FirstName} {LastName} ({Email}) at {PublishedOn}. Message: {Message}",
            notification.FirstName,
            notification.LastName,
            notification.Email,
            notification.PublishedOn,
            notification.Message);

        return Task.CompletedTask;
    }
}
