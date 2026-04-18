using Modules.Quiz.Application.Question.QuestionSet.Dtos;
using Shared.Core;
using Shared.Core.Caching;

namespace Modules.Quiz.Application.Question.QuestionSet.Update;

public sealed record ToggleVisibilityCommand(long QuestionSetId, bool IsPublic)
    : ICommand<Result<QuestionSetResponse>>, ICacheInvalidatingCommand
{
    public string[] CacheKeysToInvalidate => [
        $"{CacheKeys.QuestionSets}:all:",
        $"{CacheKeys.QuestionSets}:id:{QuestionSetId}"
    ];
}
