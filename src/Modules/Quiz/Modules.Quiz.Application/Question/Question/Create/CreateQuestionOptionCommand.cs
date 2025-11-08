namespace Modules.Quiz.Application.Question.Question.Create;

public sealed record CreateQuestionOptionCommand(string OptionText, bool IsAnswer = false);