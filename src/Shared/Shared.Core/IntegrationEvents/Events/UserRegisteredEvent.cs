namespace Shared.Core.IntegrationEvents.Events;

public sealed record UserRegisteredEvent(Guid UserId, string Email, string UserType) : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
