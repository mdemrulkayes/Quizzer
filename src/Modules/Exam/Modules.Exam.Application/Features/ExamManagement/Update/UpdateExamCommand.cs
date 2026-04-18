using Modules.Exam.Application.Features.ExamManagement.Dtos;
using Shared.Core;
using Shared.Core.Caching;

namespace Modules.Exam.Application.Features.ExamManagement.Update;

public sealed record UpdateExamCommand(
    long ExamId,
    string Title,
    string? Description,
    int DurationInMinutes,
    int TotalMarks,
    int PassingMarks,
    DateTimeOffset? ScheduledStartTime,
    DateTimeOffset? ScheduledEndTime) : ICommand<Result<ExamResponse>>, ICacheInvalidatingCommand
{
    public string[] CacheKeysToInvalidate => [$"{CacheKeys.Exams}:all:", $"{CacheKeys.Exams}:id:{ExamId}"];
}
