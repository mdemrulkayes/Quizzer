namespace Shared.Core.IntegrationEvents.Events;

public sealed record ExamGradedEvent(
    long ExamId,
    long ExamAttemptId,
    Guid UserId,
    int TotalScore,
    int TotalMarks,
    bool IsPassed) : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
