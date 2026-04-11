using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Exam.Core.Enums;
using Modules.Exam.Core.ExamAggregate;
using Modules.Exam.Infrastructure.Persistence;
using Shared.Core;

namespace Modules.Exam.Application.Features.ExamTaking.Answer;

public sealed record SubmitAnswerCommand(
    long ExamId,
    long QuestionId,
    long? SelectedOptionId) : ICommand<Result<bool>>;

internal sealed class SubmitAnswerCommandHandler(
    IExamRepository examRepository,
    IUser currentUser,
    ITimeProvider timeProvider,
    ExamModuleDbContext dbContext,
    [FromKeyedServices(ModuleKeys.Exam)] IUnitOfWork unitOfWork)
    : ICommandHandler<SubmitAnswerCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(SubmitAnswerCommand command, CancellationToken cancellationToken)
    {
        var exam = await examRepository.FirstOrDefaultAsync(e => e.ExamId == command.ExamId);
        if (exam is null)
            return ExamErrors.ExamNotFound;

        var userId = Guid.Parse(currentUser.Id!);

        var attempt = await dbContext.ExamAttempts
            .FirstOrDefaultAsync(a => a.ExamId == command.ExamId
                && a.UserId == userId
                && a.Status == ExamAttemptStatus.InProgress, cancellationToken);

        if (attempt is null)
            return ExamErrors.AttemptNotFound;

        if (attempt.IsExpired(exam.DurationInMinutes, timeProvider))
            return ExamErrors.AttemptExpired;

        // Check if already answered this question — update if so
        var existingAnswer = await dbContext.ExamAnswers
            .FirstOrDefaultAsync(a => a.ExamAttemptId == attempt.ExamAttemptId
                && a.QuestionId == command.QuestionId, cancellationToken);

        if (existingAnswer is not null)
        {
            dbContext.ExamAnswers.Remove(existingAnswer);
        }

        var answerResult = ExamAnswer.Create(attempt.ExamAttemptId, command.QuestionId, command.SelectedOptionId);
        if (!answerResult.IsSuccess)
            return answerResult.Error;

        dbContext.ExamAnswers.Add(answerResult.Value!);
        await unitOfWork.CommitAsync(cancellationToken);

        return true;
    }
}
