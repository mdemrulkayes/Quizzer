using Modules.Exam.Application.Features.ExamManagement.Dtos;
using Shared.Core;
using Shared.Core.Caching;

namespace Modules.Exam.Application.Features.ExamManagement.Create;

public sealed record CreateExamCommand(
    string Title,
    string? Description,
    long QuestionSetId,
    int DurationInMinutes,
    int TotalMarks,
    int PassingMarks,
    DateTimeOffset? ScheduledStartTime,
    DateTimeOffset? ScheduledEndTime) : ICommand<Result<ExamResponse>>, ICacheInvalidatingCommand
{
    public string[] CacheKeysToInvalidate => [$"{CacheKeys.Exams}:all:"];
}
