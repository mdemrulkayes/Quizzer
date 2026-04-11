using Modules.Quiz.Application.Question.Question.Dtos;
using Shared.Core;
using Shared.Core.Caching;

namespace Modules.Quiz.Application.Question.Question.Query;
public sealed record GetQuestionByIdQuery(long QuestionId) : IQuery<Result<QuestionResponse>>, ICacheableQuery
{
    public string CacheKey => $"{CacheKeys.Questions}:id:{QuestionId}";
}