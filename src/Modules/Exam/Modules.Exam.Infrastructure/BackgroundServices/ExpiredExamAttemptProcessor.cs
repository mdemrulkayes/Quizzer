using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Modules.Exam.Core.Enums;
using Modules.Exam.Core.Services;
using Modules.Exam.Infrastructure.Persistence;
using Shared.Core;
using Shared.Core.IntegrationEvents;
using Shared.Core.IntegrationEvents.Events;

namespace Modules.Exam.Infrastructure.BackgroundServices;

internal sealed class ExpiredExamAttemptProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<ExpiredExamAttemptProcessor> logger) : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ExpiredExamAttemptProcessor started. Polling every {Interval}s for expired attempts",
            PollingInterval.TotalSeconds);

        using var timer = new PeriodicTimer(PollingInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ProcessExpiredAttemptsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled error in ExpiredExamAttemptProcessor polling cycle");
            }
        }
    }

    private async Task ProcessExpiredAttemptsAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ExamModuleDbContext>();
        var timeProvider = scope.ServiceProvider.GetRequiredService<ITimeProvider>();
        var gradingService = scope.ServiceProvider.GetRequiredService<IExamGradingService>();
        var eventPublisher = scope.ServiceProvider.GetRequiredService<IIntegrationEventPublisher>();

        var now = timeProvider.TimeNow;

        var expiredAttempts = await dbContext.ExamAttempts
            .Include(a => a.Answers)
            .Include(a => a.Exam)
            .Where(a => a.Status == ExamAttemptStatus.InProgress
                        && a.Exam != null
                        && now > a.StartedAt.AddMinutes(a.Exam.DurationInMinutes))
            .ToListAsync(cancellationToken);

        if (expiredAttempts.Count == 0)
            return;

        logger.LogInformation("Found {Count} expired exam attempt(s) to auto-complete", expiredAttempts.Count);

        foreach (var attempt in expiredAttempts)
        {
            try
            {
                attempt.MarkTimedOut(timeProvider);
                await gradingService.GradeAttemptAsync(attempt, attempt.Exam!, cancellationToken);

                dbContext.ExamAttempts.Update(attempt);
                await dbContext.SaveChangesAsync(cancellationToken);

                await eventPublisher.PublishAsync(
                    new ExamGradedEvent(
                        attempt.Exam!.ExamId,
                        attempt.ExamAttemptId,
                        attempt.UserId,
                        attempt.TotalScore ?? 0,
                        attempt.Exam.TotalMarks,
                        attempt.IsPassed ?? false),
                    cancellationToken);

                logger.LogInformation(
                    "Auto-completed expired exam attempt {AttemptId} for exam {ExamId} (user {UserId}). Score: {Score}/{TotalMarks}",
                    attempt.ExamAttemptId, attempt.ExamId, attempt.UserId,
                    attempt.TotalScore ?? 0, attempt.Exam.TotalMarks);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Failed to auto-complete expired exam attempt {AttemptId} for exam {ExamId}",
                    attempt.ExamAttemptId, attempt.ExamId);
            }
        }
    }
}
