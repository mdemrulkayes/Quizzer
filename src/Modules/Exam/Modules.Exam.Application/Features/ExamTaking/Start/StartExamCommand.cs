using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Exam.Core.Enums;
using Modules.Exam.Core.ExamAggregate;
using Modules.Exam.Infrastructure.Persistence;
using Shared.Core;
using Shared.Core.ModuleServices;

namespace Modules.Exam.Application.Features.ExamTaking.Start;

public sealed record StartExamCommand(long ExamId) : ICommand<Result<ExamAttemptStartResponse>>;

public sealed record ExamAttemptStartResponse(
    long ExamAttemptId,
    long ExamId,
    string ExamTitle,
    int DurationInMinutes,
    DateTimeOffset StartedAt,
    DateTimeOffset ExpiresAt,
    IReadOnlyCollection<ExamQuestionResponse> Questions);

public sealed record ExamQuestionResponse(
    long QuestionId,
    string QuestionText,
    int? Marks,
    IReadOnlyCollection<ExamQuestionOptionResponse> Options);

public sealed record ExamQuestionOptionResponse(
    long OptionId,
    string OptionText);

internal sealed class StartExamCommandHandler(
    IExamRepository examRepository,
    IExamAttemptRepository attemptRepository,
    [FromKeyedServices(ModuleKeys.Exam)] IUnitOfWork unitOfWork,
    IUser currentUser,
    ITimeProvider timeProvider,
    ExamModuleDbContext examDbContext,
    IQuestionQueryService questionQueryService)
    : ICommandHandler<StartExamCommand, Result<ExamAttemptStartResponse>>
{
    public async Task<Result<ExamAttemptStartResponse>> Handle(StartExamCommand command, CancellationToken cancellationToken)
    {
        var exam = await examRepository.FirstOrDefaultAsync(e => e.ExamId == command.ExamId);
        if (exam is null)
            return ExamErrors.ExamNotFound;

        if (!exam.IsPublished)
            return ExamErrors.ExamNotPublished;

        if (exam.ScheduledStartTime.HasValue && timeProvider.TimeNow < exam.ScheduledStartTime.Value)
            return ExamErrors.ExamNotInSchedule;

        if (exam.ScheduledEndTime.HasValue && timeProvider.TimeNow > exam.ScheduledEndTime.Value)
            return ExamErrors.ExamNotInSchedule;

        var userId = Guid.Parse(currentUser.Id!);

        // Check for existing in-progress attempt
        var existingAttempt = await examDbContext.ExamAttempts
            .AnyAsync(a => a.ExamId == command.ExamId && a.UserId == userId
                && a.Status == ExamAttemptStatus.InProgress, cancellationToken);

        if (existingAttempt)
            return ExamErrors.AttemptAlreadyInProgress;

        var attemptResult = ExamAttempt.Create(command.ExamId, userId, timeProvider);
        if (!attemptResult.IsSuccess)
            return attemptResult.Error;

        var attempt = attemptResult.Value!;
        attemptRepository.Add(attempt);
        await unitOfWork.CommitAsync(cancellationToken);

        // Fetch questions via IQuestionQueryService (no cross-schema SQL)
        var questions = await questionQueryService.GetQuestionsBySetIdAsync(exam.QuestionSetId, cancellationToken);
        var questionIds = questions.Select(q => q.QuestionId).ToList();
        var options = await questionQueryService.GetOptionsByQuestionIdsAsync(questionIds, cancellationToken);

        var optionsByQuestion = options.GroupBy(o => o.QuestionId).ToDictionary(g => g.Key, g => g.ToList());

        var examQuestions = questions.Select(q => new ExamQuestionResponse(
            q.QuestionId,
            q.AskedQuestion,
            q.QuestionMark,
            optionsByQuestion.GetValueOrDefault(q.QuestionId, [])
                .Select(o => new ExamQuestionOptionResponse(o.QuestionOptionId, o.OptionText))
                .ToList()
                .AsReadOnly()
        )).ToList().AsReadOnly();

        return new ExamAttemptStartResponse(
            attempt.ExamAttemptId,
            exam.ExamId,
            exam.Title,
            exam.DurationInMinutes,
            attempt.StartedAt,
            attempt.StartedAt.AddMinutes(exam.DurationInMinutes),
            examQuestions);
    }
}
