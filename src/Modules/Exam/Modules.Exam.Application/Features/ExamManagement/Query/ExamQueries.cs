using Modules.Exam.Application.Features.ExamManagement.Dtos;
using Shared.Core;
using Shared.Core.Caching;

namespace Modules.Exam.Application.Features.ExamManagement.Query;

public sealed record GetAllExamsQuery(int PageNumber = 1, int PageSize = 10)
    : IQuery<Result<PaginatedList<ExamResponse>>>, ICacheableQuery
{
    public string CacheKey => $"{CacheKeys.Exams}:all:{PageNumber}:{PageSize}";
}

public sealed record GetExamByIdQuery(long ExamId) : IQuery<Result<ExamResponse>>, ICacheableQuery
{
    public string CacheKey => $"{CacheKeys.Exams}:id:{ExamId}";
}
