using Modules.Quiz.Application.Question.Question.Create;
using Modules.Quiz.Application.Question.QuestionSet.Dtos;
using Shared.Core;
using Shared.Core.Caching;

namespace Modules.Quiz.Application.Question.QuestionSet.Create;

public sealed record CreateQuestionSetCommand(string Name, string? SetCode , string? Details, List<CreateQuestionCommand> Questions) : ICommand<Result<QuestionSetResponse>>, ICacheInvalidatingCommand
{
    public string[] CacheKeysToInvalidate => [$"{CacheKeys.QuestionSets}:all:"];
}
