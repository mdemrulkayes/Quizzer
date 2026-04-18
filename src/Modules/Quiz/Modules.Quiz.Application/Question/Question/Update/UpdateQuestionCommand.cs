using Modules.Quiz.Application.Question.Question.Dtos;
using Shared.Core;
using Shared.Core.Caching;

namespace Modules.Quiz.Application.Question.Question.Update;

public sealed record UpdateQuestionCommand(long QuestionId, string Question, string Details, int? Mark) : ICommand<Result<QuestionResponse>>, ICacheInvalidatingCommand
{
    public string[] CacheKeysToInvalidate => [$"{CacheKeys.Questions}:all:", $"{CacheKeys.Questions}:id:{QuestionId}"];
}
