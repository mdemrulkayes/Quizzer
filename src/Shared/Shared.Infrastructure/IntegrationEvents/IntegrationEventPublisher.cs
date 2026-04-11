using Shared.Core.IntegrationEvents;

namespace Shared.Infrastructure.IntegrationEvents;

internal sealed class IntegrationEventPublisher(IntegrationEventChannel channel) : IIntegrationEventPublisher
{
    public async ValueTask PublishAsync(IIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        await channel.Writer.WriteAsync(@event, cancellationToken);
    }
}
