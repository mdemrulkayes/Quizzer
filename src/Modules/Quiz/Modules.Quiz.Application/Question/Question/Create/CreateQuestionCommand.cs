using Modules.Quiz.Application.Question.Question.Dtos;
using Modules.Quiz.Core.QuestionAggregate;
using Shared.Core;

namespace Modules.Quiz.Application.Question.Question.Create;

public sealed record CreateQuestionCommand(string Question, string Details, int? Mark, List<QuestionOption> QuestionOptions) : ICommand<Result<QuestionResponse>>;
