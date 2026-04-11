using Microsoft.EntityFrameworkCore;
using Modules.Exam.Core.ExamAggregate;
using Modules.Exam.Infrastructure.Persistence;

namespace Modules.Exam.Application.Services;

internal sealed class ExamGradingService(ExamModuleDbContext dbContext) : IExamGradingService
{
    public async Task GradeAttemptAsync(ExamAttempt attempt, Core.ExamAggregate.Exam exam, CancellationToken cancellationToken = default)
    {
        // Fetch correct answers from the Question module schema
        var correctOptionIds = await dbContext.Database
            .SqlQueryRaw<CorrectOptionRow>(
                @"SELECT qo.QuestionOptionId, qo.QuestionId
                  FROM [Question].[QuestionOptions] qo
                  INNER JOIN [Question].[Questions] q ON qo.QuestionId = q.QuestionId
                  WHERE q.QuestionSetId = {0} AND qo.IsAnswer = 1 AND qo.IsDeleted = 0 AND q.IsDeleted = 0",
                exam.QuestionSetId)
            .ToListAsync(cancellationToken);

        var correctByQuestion = correctOptionIds
            .GroupBy(c => c.QuestionId)
            .ToDictionary(g => g.Key, g => g.Select(c => c.QuestionOptionId).ToHashSet());

        // Fetch question marks
        var questionMarks = await dbContext.Database
            .SqlQueryRaw<QuestionMarkRow>(
                @"SELECT QuestionId, QuestionMark FROM [Question].[Questions] 
                  WHERE QuestionSetId = {0} AND IsDeleted = 0",
                exam.QuestionSetId)
            .ToListAsync(cancellationToken);

        var marksByQuestion = questionMarks.ToDictionary(q => q.QuestionId, q => q.QuestionMark ?? 1);

        var totalScore = 0;

        foreach (var answer in attempt.Answers)
        {
            var isCorrect = false;
            var marksAwarded = 0;

            if (answer.SelectedOptionId.HasValue &&
                correctByQuestion.TryGetValue(answer.QuestionId, out var correctOptions))
            {
                isCorrect = correctOptions.Contains(answer.SelectedOptionId.Value);
                if (isCorrect)
                {
                    marksAwarded = marksByQuestion.GetValueOrDefault(answer.QuestionId, 1);
                }
            }

            answer.SetGradingResult(isCorrect, marksAwarded);
            totalScore += marksAwarded;
        }

        attempt.SetGradingResult(totalScore, totalScore >= exam.PassingMarks);
    }
}

internal sealed class CorrectOptionRow
{
    public long QuestionOptionId { get; set; }
    public long QuestionId { get; set; }
}

internal sealed class QuestionMarkRow
{
    public long QuestionId { get; set; }
    public int? QuestionMark { get; set; }
}
