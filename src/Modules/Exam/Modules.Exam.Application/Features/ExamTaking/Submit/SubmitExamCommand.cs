using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Exam.Core.Enums;
using Modules.Exam.Core.ExamAggregate;
using Modules.Exam.Infrastructure.Persistence;
using Shared.Core;
using Shared.Core.IntegrationEvents;
using Shared.Core.IntegrationEvents.Events;

namespace Modules.Exam.Application.Features.ExamTaking.Submit;

public sealed record SubmitExamCommand(long ExamId) : ICommand<Result<ExamSubmitResponse>>;

public sealed record ExamSubmitResponse(
    long ExamAttemptId,
    int TotalScore,
    int TotalMarks,
    int PassingMarks,
    bool IsPassed,
    string Status);

internal sealed class SubmitExamCommandHandler(
    IExamRepository examRepository,
    IUser currentUser,
    ITimeProvider timeProvider,
    IExamGradingService gradingService,
    ExamModuleDbContext dbContext,
    [FromKeyedServices(ModuleKeys.Exam)] IUnitOfWork unitOfWork,
    IIntegrationEventPublisher eventPublisher)
    : ICommandHandler<SubmitExamCommand, Result<ExamSubmitResponse>>
{
    public async Task<Result<ExamSubmitResponse>> Handle(SubmitExamCommand command, CancellationToken cancellationToken)
    {
        var exam = await examRepository.FirstOrDefaultAsync(e => e.ExamId == command.ExamId);
        if (exam is null)
            return ExamErrors.ExamNotFound;

        var userId = Guid.Parse(currentUser.Id!);

        var attempt = await dbContext.ExamAttempts
            .Include(a => a.Answers)
            .FirstOrDefaultAsync(a => a.ExamId == command.ExamId
                && a.UserId == userId
                && a.Status == ExamAttemptStatus.InProgress, cancellationToken);

        if (attempt is null)
            return ExamErrors.AttemptNotFound;

        // Mark as timed out if expired, otherwise as submitted
        if (attempt.IsExpired(exam.DurationInMinutes, timeProvider))
            attempt.MarkTimedOut(timeProvider);
        else
            attempt.Submit(timeProvider);

        // Grade the attempt
        await gradingService.GradeAttemptAsync(attempt, exam, cancellationToken);

        dbContext.ExamAttempts.Update(attempt);
        await unitOfWork.CommitAsync(cancellationToken);

        await eventPublisher.PublishAsync(
            new ExamGradedEvent(
                exam.ExamId,
                attempt.ExamAttemptId,
                userId,
                attempt.TotalScore ?? 0,
                exam.TotalMarks,
                attempt.IsPassed ?? false),
            cancellationToken);

        return new ExamSubmitResponse(
            attempt.ExamAttemptId,
            attempt.TotalScore ?? 0,
            exam.TotalMarks,
            exam.PassingMarks,
            attempt.IsPassed ?? false,
            attempt.Status.ToString());
    }
}
