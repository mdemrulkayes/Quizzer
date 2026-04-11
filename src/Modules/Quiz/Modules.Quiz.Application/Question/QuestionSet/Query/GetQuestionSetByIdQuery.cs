using Modules.Quiz.Application.Question.QuestionSet.Dtos;
using Shared.Core;
using Shared.Core.Caching;

namespace Modules.Quiz.Application.Question.QuestionSet.Query;
public sealed record GetQuestionSetByIdQuery(long QuestionSetId) : IQuery<Result<QuestionSetResponse>>, ICacheableQuery
{
    public string CacheKey => $"{CacheKeys.QuestionSets}:id:{QuestionSetId}";
}