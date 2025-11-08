using Modules.Quiz.Application.Question.Question.Dtos;

namespace Modules.Quiz.Application.Question.QuestionSet.Dtos;

public sealed record QuestionSetResponse(long QuestionSetId, string Name, string? SetCode, string? Details, List<QuestionResponse> Questions);
