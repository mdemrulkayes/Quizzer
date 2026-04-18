namespace Modules.AI.Application.Dtos;

public sealed record InterviewPrepMaterialDetailDto(
    Guid Id,
    string JobTitle,
    string JobDescription,
    List<string> KeyTopics,
    List<ReadingMaterialDto> ReadingMaterials,
    List<PracticeQuestionDto> PracticeQuestions,
    List<string> PreparationTips,
    DateTimeOffset CreatedAt);

public sealed record ReadingMaterialDto(string Title, string Description, string? Url, string Type);

public sealed record PracticeQuestionDto(string Question, string Hint);
