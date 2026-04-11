using Modules.Exam.Core.Enums;
using Shared.Core;

namespace Modules.Exam.Core.ExamAggregate;

public sealed class ExamAttempt : BaseAuditableEntity
{
    public long ExamAttemptId { get; private set; }
    public long ExamId { get; private set; }
    public Exam? Exam { get; private set; }
    public Guid UserId { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? SubmittedAt { get; private set; }
    public ExamAttemptStatus Status { get; private set; }
    public int? TotalScore { get; private set; }
    public bool? IsPassed { get; private set; }

    private readonly List<ExamAnswer> _answers = [];
    public IReadOnlyCollection<ExamAnswer> Answers => _answers.AsReadOnly();

    private ExamAttempt() { }

    private ExamAttempt(long examId, Guid userId, DateTimeOffset startedAt)
    {
        ExamId = examId;
        UserId = userId;
        StartedAt = startedAt;
        Status = ExamAttemptStatus.InProgress;
    }

    public static Result<ExamAttempt> Create(long examId, Guid userId, ITimeProvider timeProvider)
    {
        return new ExamAttempt(examId, userId, timeProvider.TimeNow);
    }

    public void AddAnswer(ExamAnswer answer)
    {
        _answers.Add(answer);
    }

    public Result<ExamAttempt> Submit(ITimeProvider timeProvider)
    {
        if (Status != ExamAttemptStatus.InProgress)
            return ExamErrors.AttemptNotInProgress;

        SubmittedAt = timeProvider.TimeNow;
        Status = ExamAttemptStatus.Submitted;
        return this;
    }

    public void MarkTimedOut(ITimeProvider timeProvider)
    {
        SubmittedAt = timeProvider.TimeNow;
        Status = ExamAttemptStatus.TimedOut;
    }

    public void SetGradingResult(int totalScore, bool isPassed)
    {
        TotalScore = totalScore;
        IsPassed = isPassed;
        Status = ExamAttemptStatus.Graded;
    }

    public bool IsExpired(int durationInMinutes, ITimeProvider timeProvider)
    {
        return timeProvider.TimeNow > StartedAt.AddMinutes(durationInMinutes);
    }
}
