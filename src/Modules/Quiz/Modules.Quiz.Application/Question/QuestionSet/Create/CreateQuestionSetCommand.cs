using Modules.Quiz.Application.Question.Question.Create;
using Modules.Quiz.Application.Question.QuestionSet.Dtos;
using Shared.Core;

namespace Modules.Quiz.Application.Question.QuestionSet.Create;

public sealed record CreateQuestionSetCommand(string Name, string? SetCode , string? Details, List<CreateQuestionCommand> Questions) : ICommand<Result<QuestionSetResponse>>;
