namespace Modules.Quiz.Application.Question.Question.Dtos;

public sealed record QuestionResponse(long QuestionId, string Question, string Details, int? Mark, List<QuestionOptionResponse> QuestionOptions);
