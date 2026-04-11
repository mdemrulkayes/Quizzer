using Modules.Quiz.Application.Question.Question.Dtos;
using Shared.Core;
using Shared.Core.Caching;

namespace Modules.Quiz.Application.Question.Question.Create;

public sealed record CreateQuestionCommand(string Question, string Details, int? Mark, List<CreateQuestionOptionCommand> QuestionOptions) : ICommand<Result<QuestionResponse>>, ICacheInvalidatingCommand
{
    public string[] CacheKeysToInvalidate => [$"{CacheKeys.Questions}:all:"];
}
