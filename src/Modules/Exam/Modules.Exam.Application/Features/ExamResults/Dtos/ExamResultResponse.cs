namespace Modules.Exam.Application.Features.ExamResults.Dtos;

public sealed record ExamResultResponse(
    long ExamAttemptId,
    long ExamId,
    string ExamTitle,
    DateTimeOffset StartedAt,
    DateTimeOffset? SubmittedAt,
    string Status,
    int? TotalScore,
    int TotalMarks,
    int PassingMarks,
    bool? IsPassed,
    IReadOnlyCollection<AnswerDetailResponse> Answers);

public sealed record AnswerDetailResponse(
    long QuestionId,
    string QuestionText,
    long? SelectedOptionId,
    string? SelectedOptionText,
    bool? IsCorrect,
    int? MarksAwarded);
