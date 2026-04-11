using Modules.Quiz.Application.Question.Question.Dtos;
using Shared.Application;
using Shared.Core;
using Shared.Core.Caching;

namespace Modules.Quiz.Application.Question.Question.Query;

public sealed record GetAllQuestionQuery(
    string? SearchText = null,
    long? QuestionSetId = null,
    int PageNumber = 1,
    int PageSize = 10)
    : QueryStringParameter(PageNumber, PageSize), IQuery<Result<PagedListDto<QuestionResponse>>>, ICacheableQuery
{
    public string CacheKey => $"{CacheKeys.Questions}:all:{SearchText}:{QuestionSetId}:{PageNumber}:{PageSize}";
}
