using Modules.Exam.Core.Enums;

namespace Modules.Exam.Application.Features.ExamManagement.Dtos;

public sealed record ExamResponse(
    long ExamId,
    string Title,
    string? Description,
    long QuestionSetId,
    int DurationInMinutes,
    int TotalMarks,
    int PassingMarks,
    bool IsPublished,
    DateTimeOffset? ScheduledStartTime,
    DateTimeOffset? ScheduledEndTime);

public sealed record ExamAttemptResponse(
    long ExamAttemptId,
    long ExamId,
    string ExamTitle,
    Guid UserId,
    DateTimeOffset StartedAt,
    DateTimeOffset? SubmittedAt,
    ExamAttemptStatus Status,
    int? TotalScore,
    bool? IsPassed);
