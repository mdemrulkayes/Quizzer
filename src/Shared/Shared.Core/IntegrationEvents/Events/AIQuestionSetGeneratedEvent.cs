namespace Shared.Core.IntegrationEvents.Events;

public sealed record AIQuestionSetGeneratedEvent : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;

    public required string Title { get; init; }
    public required string Source { get; init; }
    public required string? Complexity { get; init; }
    public required int? ExperienceYears { get; init; }
    public required string? ExpertiseFields { get; init; }
    public required bool IsPublic { get; init; }
    public required Guid CreatedByUserId { get; init; }
    public required List<GeneratedQuestionData> Questions { get; init; }
}

public sealed record GeneratedQuestionData
{
    public required int Sequence { get; init; }
    public required string Text { get; init; }
    public required string Type { get; init; }
    public required List<GeneratedOptionData> Options { get; init; }
    public required string? CorrectOptionId { get; init; }
    public required string? Explanation { get; init; }
    public required List<string> Tags { get; init; }
    public required int DifficultyScore { get; init; }
}

public sealed record GeneratedOptionData
{
    public required string Id { get; init; }
    public required string Text { get; init; }
}
