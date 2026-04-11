namespace Shared.Core.IntegrationEvents;

public interface IIntegrationEventPublisher
{
    ValueTask PublishAsync(IIntegrationEvent @event, CancellationToken cancellationToken = default);
}
