using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Core.IntegrationEvents;

namespace Shared.Infrastructure.IntegrationEvents;

internal sealed class IntegrationEventProcessor(
    IntegrationEventChannel channel,
    IServiceScopeFactory scopeFactory,
    ILogger<IntegrationEventProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("IntegrationEventProcessor started. Listening for integration events...");

        await foreach (var @event in channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                logger.LogInformation(
                    "Processing integration event {EventType} (Id: {EventId})",
                    @event.GetType().Name, @event.EventId);

                await DispatchEventAsync(@event, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Error processing integration event {EventType} (Id: {EventId})",
                    @event.GetType().Name, @event.EventId);
            }
        }
    }

    private async Task DispatchEventAsync(IIntegrationEvent @event, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();

        var eventType = @event.GetType();
        var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
        var handlers = scope.ServiceProvider.GetServices(handlerType);

        var handleMethod = handlerType.GetMethod("HandleAsync");
        if (handleMethod is null)
        {
            logger.LogWarning("No HandleAsync method found for handler type {HandlerType}", handlerType.Name);
            return;
        }

        foreach (var handler in handlers)
        {
            try
            {
                logger.LogDebug(
                    "Dispatching {EventType} to handler {HandlerType}",
                    eventType.Name, handler!.GetType().Name);

                var task = (Task)handleMethod.Invoke(handler, [@event, cancellationToken])!;
                await task;
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Handler {HandlerType} failed for event {EventType}",
                    handler!.GetType().Name, eventType.Name);
            }
        }
    }
}
