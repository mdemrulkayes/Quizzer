using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Exam.Core.Enums;
using Modules.Exam.Infrastructure.Persistence;
using Shared.Core;
using Shared.Core.IntegrationEvents;
using Shared.Core.IntegrationEvents.Events;

namespace Modules.Exam.Application.Features.IntegrationEventHandlers;

internal sealed class UserDeletedHandler(
    ExamModuleDbContext dbContext,
    ITimeProvider timeProvider,
    ILogger<UserDeletedHandler> logger) : IIntegrationEventHandler<UserDeletedEvent>
{
    public async Task HandleAsync(UserDeletedEvent @event, CancellationToken cancellationToken = default)
    {
        var inProgressAttempts = await dbContext.ExamAttempts
            .Where(a => a.UserId == @event.UserId && a.Status == ExamAttemptStatus.InProgress)
            .ToListAsync(cancellationToken);

        if (inProgressAttempts.Count == 0)
        {
            logger.LogInformation(
                "No in-progress exam attempts found for deleted user {UserId}",
                @event.UserId);
            return;
        }

        foreach (var attempt in inProgressAttempts)
        {
            attempt.MarkTimedOut(timeProvider);
            logger.LogWarning(
                "Cancelled exam attempt {AttemptId} for deleted user {UserId}",
                attempt.ExamAttemptId, @event.UserId);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Cancelled {Count} in-progress attempt(s) for deleted user {UserId}",
            inProgressAttempts.Count, @event.UserId);
    }
}
