using Shared.Core;
using Shared.Core.Caching;

namespace Modules.Quiz.Application.Question.Question.Delete;
public sealed record DeleteQuestionCommand(long QuestionId) : ICommand<Result<bool>>, ICacheInvalidatingCommand
{
    public string[] CacheKeysToInvalidate => [$"{CacheKeys.Questions}:id:{QuestionId}"];
}
