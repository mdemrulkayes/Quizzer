namespace Shared.Core.IntegrationEvents.Events;

public sealed record UserDeletedEvent(Guid UserId) : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
