namespace Modules.AI.Application.Dtos;

public sealed record GenerationHistoryItemDto(
    Guid Id,
    string Source,
    string OutputType,
    string Status,
    string? ErrorMessage,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);
