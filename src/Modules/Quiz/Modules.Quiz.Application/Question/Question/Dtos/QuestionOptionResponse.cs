namespace Modules.Quiz.Application.Question.Question.Dtos;

public sealed record QuestionOptionResponse(long QuestionOptionId, string OptionText, bool IsCorrect, string? OptionIdentifier);
