namespace Modules.AI.Application.Dtos;

public sealed record InterviewPrepMaterialDto(
    Guid Id,
    string JobTitle,
    List<string> KeyTopics,
    DateTimeOffset CreatedAt);
