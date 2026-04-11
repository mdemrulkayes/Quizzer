namespace Shared.Core.IntegrationEvents.Events;

public sealed record QuestionSetDeletedEvent(long QuestionSetId) : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
