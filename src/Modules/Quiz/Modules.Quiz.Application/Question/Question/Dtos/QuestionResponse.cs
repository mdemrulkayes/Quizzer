using Modules.Quiz.Core.Enums;

namespace Modules.Quiz.Application.Question.Question.Dtos;

public sealed record QuestionResponse(
    long QuestionId,
    string Question,
    string Details,
    int? Mark,
    QuestionType QuestionType,
    string? Explanation,
    int? DifficultyScore,
    int? Sequence,
    List<QuestionOptionResponse> QuestionOptions);
