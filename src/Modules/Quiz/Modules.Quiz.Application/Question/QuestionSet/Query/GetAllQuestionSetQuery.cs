using Modules.Quiz.Application.Question.QuestionSet.Dtos;
using Modules.Quiz.Core.Enums;
using Shared.Application;
using Shared.Core;
using Shared.Core.Caching;

namespace Modules.Quiz.Application.Question.QuestionSet.Query;

public sealed record GetAllQuestionSetQuery(
    string? SearchName = null,
    long? TagId = null,
    QuestionSetSource? Source = null,
    bool? IsPublic = null,
    Complexity? ComplexityFilter = null,
    string? SortBy = null,
    int PageNumber = 1,
    int PageSize = 10)
    : QueryStringParameter(PageNumber, PageSize), IQuery<Result<PagedListDto<QuestionSetResponse>>>, ICacheableQuery
{
    public string CacheKey => $"{CacheKeys.QuestionSets}:all:{SearchName}:{TagId}:{Source}:{IsPublic}:{ComplexityFilter}:{SortBy}:{PageNumber}:{PageSize}";
}
