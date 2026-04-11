using Modules.Exam.Core.ExamAggregate;
using Modules.Exam.Core.Services;
using Shared.Core.ModuleServices;

namespace Modules.Exam.Application.Services;

internal sealed class ExamGradingService(IQuestionQueryService questionQueryService) : IExamGradingService
{
    public async Task GradeAttemptAsync(ExamAttempt attempt, Core.ExamAggregate.Exam exam, CancellationToken cancellationToken = default)
    {
        // Fetch correct answers via IQuestionQueryService (no cross-schema SQL)
        var correctAnswers = await questionQueryService.GetCorrectAnswersBySetIdAsync(exam.QuestionSetId, cancellationToken);

        var correctByQuestion = correctAnswers
            .GroupBy(c => c.QuestionId)
            .ToDictionary(g => g.Key, g => g.Select(c => c.QuestionOptionId).ToHashSet());

        // Fetch question marks
        var questions = await questionQueryService.GetQuestionsBySetIdAsync(exam.QuestionSetId, cancellationToken);
        var marksByQuestion = questions.ToDictionary(q => q.QuestionId, q => q.QuestionMark ?? 1);

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
