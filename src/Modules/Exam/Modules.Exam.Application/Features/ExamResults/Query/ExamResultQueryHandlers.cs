using Microsoft.EntityFrameworkCore;
using Modules.Exam.Application.Features.ExamManagement.Dtos;
using Modules.Exam.Application.Features.ExamResults.Dtos;
using Modules.Exam.Application.Services;
using Modules.Exam.Core.Enums;
using Modules.Exam.Core.ExamAggregate;
using Modules.Exam.Infrastructure.Persistence;
using Shared.Core;
using Shared.Core.Extensions;

namespace Modules.Exam.Application.Features.ExamResults.Query;

public sealed record GetMyExamResultQuery(long ExamId) : IQuery<Result<ExamResultResponse>>;

public sealed record GetExamResultsQuery(long ExamId, int PageNumber = 1, int PageSize = 10)
    : IQuery<Result<PaginatedList<ExamAttemptResponse>>>;

public sealed record GetMyAllResultsQuery(int PageNumber = 1, int PageSize = 10)
    : IQuery<Result<PaginatedList<ExamAttemptResponse>>>;

internal sealed class GetMyExamResultQueryHandler(
    ExamModuleDbContext dbContext,
    IUser currentUser,
    ITimeProvider timeProvider,
    IExamGradingService gradingService,
    IUnitOfWork unitOfWork)
    : IQueryHandler<GetMyExamResultQuery, Result<ExamResultResponse>>
{
    public async Task<Result<ExamResultResponse>> Handle(GetMyExamResultQuery request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(currentUser.Id!);

        var attempt = await dbContext.ExamAttempts
            .Include(a => a.Answers)
            .Include(a => a.Exam)
            .Where(a => a.ExamId == request.ExamId && a.UserId == userId)
            .OrderByDescending(a => a.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (attempt is null)
            return ExamErrors.AttemptNotFound;

        // Auto-grade timed-out in-progress attempts
        if (attempt.Status == ExamAttemptStatus.InProgress && attempt.Exam != null &&
            attempt.IsExpired(attempt.Exam.DurationInMinutes, timeProvider))
        {
            attempt.MarkTimedOut(timeProvider);
            await gradingService.GradeAttemptAsync(attempt, attempt.Exam, cancellationToken);
            dbContext.ExamAttempts.Update(attempt);
            await unitOfWork.CommitAsync(cancellationToken);
        }

        var exam = attempt.Exam!;

        // Build answer details with question text from Quiz module
        var questionIds = attempt.Answers.Select(a => a.QuestionId).ToList();
        var questions = await dbContext.Database
            .SqlQueryRaw<QuestionTextRow>(
                @"SELECT QuestionId, AskedQuestion FROM [Question].[Questions] WHERE IsDeleted = 0")
            .ToListAsync(cancellationToken);

        var questionTextMap = questions.ToDictionary(q => q.QuestionId, q => q.AskedQuestion);

        var optionIds = attempt.Answers.Where(a => a.SelectedOptionId.HasValue).Select(a => a.SelectedOptionId!.Value).ToList();
        var optionTexts = await dbContext.Database
            .SqlQueryRaw<OptionTextRow>(
                @"SELECT QuestionOptionId, OptionText FROM [Question].[QuestionOptions] WHERE IsDeleted = 0")
            .ToListAsync(cancellationToken);

        var optionTextMap = optionTexts.ToDictionary(o => o.QuestionOptionId, o => o.OptionText);

        var answerDetails = attempt.Answers.Select(a => new AnswerDetailResponse(
            a.QuestionId,
            questionTextMap.GetValueOrDefault(a.QuestionId, "Unknown"),
            a.SelectedOptionId,
            a.SelectedOptionId.HasValue ? optionTextMap.GetValueOrDefault(a.SelectedOptionId.Value, "Unknown") : null,
            a.IsCorrect,
            a.MarksAwarded
        )).ToList().AsReadOnly();

        return new ExamResultResponse(
            attempt.ExamAttemptId,
            exam.ExamId,
            exam.Title,
            attempt.StartedAt,
            attempt.SubmittedAt,
            attempt.Status.ToString(),
            attempt.TotalScore,
            exam.TotalMarks,
            exam.PassingMarks,
            attempt.IsPassed,
            answerDetails);
    }
}

internal sealed class GetExamResultsQueryHandler(
    ExamModuleDbContext dbContext)
    : IQueryHandler<GetExamResultsQuery, Result<PaginatedList<ExamAttemptResponse>>>
{
    public async Task<Result<PaginatedList<ExamAttemptResponse>>> Handle(GetExamResultsQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.ExamAttempts
            .Include(a => a.Exam)
            .Where(a => a.ExamId == request.ExamId && a.Status == ExamAttemptStatus.Graded)
            .OrderByDescending(a => a.SubmittedAt)
            .Select(a => new ExamAttemptResponse(
                a.ExamAttemptId, a.ExamId, a.Exam!.Title, a.UserId,
                a.StartedAt, a.SubmittedAt, a.Status, a.TotalScore, a.IsPassed));

        return await query.ToPaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}

internal sealed class GetMyAllResultsQueryHandler(
    ExamModuleDbContext dbContext,
    IUser currentUser)
    : IQueryHandler<GetMyAllResultsQuery, Result<PaginatedList<ExamAttemptResponse>>>
{
    public async Task<Result<PaginatedList<ExamAttemptResponse>>> Handle(GetMyAllResultsQuery request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(currentUser.Id!);

        var query = dbContext.ExamAttempts
            .Include(a => a.Exam)
            .Where(a => a.UserId == userId && a.Status == ExamAttemptStatus.Graded)
            .OrderByDescending(a => a.SubmittedAt)
            .Select(a => new ExamAttemptResponse(
                a.ExamAttemptId, a.ExamId, a.Exam!.Title, a.UserId,
                a.StartedAt, a.SubmittedAt, a.Status, a.TotalScore, a.IsPassed));

        return await query.ToPaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}

internal sealed class QuestionTextRow
{
    public long QuestionId { get; set; }
    public string AskedQuestion { get; set; } = string.Empty;
}

internal sealed class OptionTextRow
{
    public long QuestionOptionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
}
