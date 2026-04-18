using Modules.Quiz.Application.Question.Question.Dtos;
using Modules.Quiz.Core.Enums;

namespace Modules.Quiz.Application.Question.QuestionSet.Dtos;

public sealed record QuestionSetResponse(
    long QuestionSetId,
    string Name,
    string? SetCode,
    string? Details,
    QuestionSetSource Source,
    bool IsPublic,
    Complexity? Complexity,
    int? ExperienceYears,
    string? ExpertiseFields,
    List<QuestionResponse> Questions);
