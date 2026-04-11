using Shared.Core;

namespace Modules.Exam.Core.ExamAggregate;

public sealed class Exam : BaseAuditableEntity, IAggregateRoot
{
    public long ExamId { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public long QuestionSetId { get; private set; }
    public int DurationInMinutes { get; private set; }
    public int TotalMarks { get; private set; }
    public int PassingMarks { get; private set; }
    public bool IsPublished { get; private set; }
    public DateTimeOffset? ScheduledStartTime { get; private set; }
    public DateTimeOffset? ScheduledEndTime { get; private set; }

    private readonly List<ExamAttempt> _attempts = [];
    public IReadOnlyCollection<ExamAttempt> Attempts => _attempts.AsReadOnly();

    private Exam(string title, string? description, long questionSetId, int durationInMinutes,
        int totalMarks, int passingMarks, DateTimeOffset? scheduledStartTime, DateTimeOffset? scheduledEndTime)
    {
        Title = title;
        Description = description;
        QuestionSetId = questionSetId;
        DurationInMinutes = durationInMinutes;
        TotalMarks = totalMarks;
        PassingMarks = passingMarks;
        IsPublished = false;
        ScheduledStartTime = scheduledStartTime;
        ScheduledEndTime = scheduledEndTime;
    }

    public static Result<Exam> Create(string title, string? description, long questionSetId,
        int durationInMinutes, int totalMarks, int passingMarks,
        DateTimeOffset? scheduledStartTime = null, DateTimeOffset? scheduledEndTime = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            return ExamErrors.TitleRequired;

        if (durationInMinutes <= 0)
            return ExamErrors.InvalidDuration;

        if (passingMarks > totalMarks)
            return ExamErrors.PassingMarksExceedTotal;

        return new Exam(title, description, questionSetId, durationInMinutes,
            totalMarks, passingMarks, scheduledStartTime, scheduledEndTime);
    }

    public Result<Exam> Update(string title, string? description, int durationInMinutes,
        int totalMarks, int passingMarks, DateTimeOffset? scheduledStartTime, DateTimeOffset? scheduledEndTime)
    {
        if (IsPublished)
            return ExamErrors.CannotModifyPublishedExam;

        Title = title;
        Description = description;
        DurationInMinutes = durationInMinutes;
        TotalMarks = totalMarks;
        PassingMarks = passingMarks;
        ScheduledStartTime = scheduledStartTime;
        ScheduledEndTime = scheduledEndTime;

        return this;
    }

    public Result<Exam> Publish()
    {
        if (IsPublished)
            return ExamErrors.AlreadyPublished;

        IsPublished = true;
        return this;
    }

    public Result<Exam> Unpublish()
    {
        if (!IsPublished)
            return ExamErrors.NotPublished;

        IsPublished = false;
        return this;
    }

    public void Delete()
    {
        IsDeleted = true;
    }
}
