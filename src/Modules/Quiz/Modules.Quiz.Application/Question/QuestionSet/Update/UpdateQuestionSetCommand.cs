using Modules.Quiz.Application.Question.QuestionSet.Dtos;
using Shared.Core;
using Shared.Core.Caching;

namespace Modules.Quiz.Application.Question.QuestionSet.Update;

public sealed record UpdateQuestionSetCommand(long QuestionSetId, string Name, string? SetCode, string? Details) : ICommand<Result<QuestionSetResponse>>, ICacheInvalidatingCommand
{
    public string[] CacheKeysToInvalidate => [$"{CacheKeys.QuestionSets}:id:{QuestionSetId}"];
}
