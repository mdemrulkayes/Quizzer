using Shared.Core;
using Shared.Core.Caching;

namespace Modules.Quiz.Application.Question.QuestionSet.Delete;
public sealed record DeleteQuestionSetCommand(long QuestionSetId) : ICommand<Result<bool>>, ICacheInvalidatingCommand
{
    public string[] CacheKeysToInvalidate => [$"{CacheKeys.QuestionSets}:all:", $"{CacheKeys.QuestionSets}:id:{QuestionSetId}"];
}
