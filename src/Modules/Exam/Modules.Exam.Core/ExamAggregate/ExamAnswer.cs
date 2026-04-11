using Shared.Core;

namespace Modules.Exam.Core.ExamAggregate;

public sealed class ExamAnswer : BaseAuditableEntity
{
    public long ExamAnswerId { get; private set; }
    public long ExamAttemptId { get; private set; }
    public ExamAttempt? ExamAttempt { get; private set; }
    public long QuestionId { get; private set; }
    public long? SelectedOptionId { get; private set; }
    public bool? IsCorrect { get; private set; }
    public int? MarksAwarded { get; private set; }

    private ExamAnswer() { }

    private ExamAnswer(long examAttemptId, long questionId, long? selectedOptionId)
    {
        ExamAttemptId = examAttemptId;
        QuestionId = questionId;
        SelectedOptionId = selectedOptionId;
    }

    public static Result<ExamAnswer> Create(long examAttemptId, long questionId, long? selectedOptionId)
    {
        return new ExamAnswer(examAttemptId, questionId, selectedOptionId);
    }

    public void SetGradingResult(bool isCorrect, int marksAwarded)
    {
        IsCorrect = isCorrect;
        MarksAwarded = marksAwarded;
    }
}
