using Microsoft.EntityFrameworkCore;
using Modules.Exam.Application.Features.ExamManagement.Dtos;
using Modules.Exam.Core.Enums;
using Modules.Exam.Core.ExamAggregate;
using Modules.Exam.Infrastructure.Persistence;
using Shared.Core;

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
    IUnitOfWork unitOfWork,
    IUser currentUser,
    ITimeProvider timeProvider,
    ExamModuleDbContext examDbContext)
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

        // Fetch questions from the Question module's schema (cross-schema read)
        var questions = await examDbContext.Database.SqlQueryRaw<QuestionRow>(
            @"SELECT q.QuestionId, q.AskedQuestion, q.QuestionMark 
              FROM [Question].[Questions] q 
              WHERE q.QuestionSetId = {0} AND q.IsDeleted = 0", exam.QuestionSetId)
            .ToListAsync(cancellationToken);

        var questionIds = questions.Select(q => q.QuestionId).ToList();

        var options = await examDbContext.Database.SqlQueryRaw<OptionRow>(
            @"SELECT qo.QuestionOptionId, qo.OptionText, qo.QuestionId 
              FROM [Question].[QuestionOptions] qo 
              WHERE qo.QuestionId IN (SELECT q.QuestionId FROM [Question].[Questions] q WHERE q.QuestionSetId = {0} AND q.IsDeleted = 0)
              AND qo.IsDeleted = 0", exam.QuestionSetId)
            .ToListAsync(cancellationToken);

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

// Internal DTOs for raw SQL queries
internal sealed class QuestionRow
{
    public long QuestionId { get; set; }
    public string AskedQuestion { get; set; } = string.Empty;
    public int? QuestionMark { get; set; }
}

internal sealed class OptionRow
{
    public long QuestionOptionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public long QuestionId { get; set; }
}
