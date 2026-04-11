using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Core.IntegrationEvents;
using Shared.Core.IntegrationEvents.Events;
using Shared.Infrastructure.IntegrationEvents;

namespace Quizzer.Api.FunctionalTest.IntegrationEvents;

public class IntegrationEventBusTests
{
    [Fact]
    public async Task PublishAsync_ShouldDeliverEventToRegisteredHandler()
    {
        // Arrange
        var handler = new TestQuestionSetDeletedHandler();
        var services = BuildServiceProvider(handler);
        var publisher = services.GetRequiredService<IIntegrationEventPublisher>();
        var hostedService = services.GetServices<IHostedService>()
            .OfType<IntegrationEventProcessor>().Single();

        using var cts = new CancellationTokenSource();
        var processorTask = hostedService.StartAsync(cts.Token);

        var @event = new QuestionSetDeletedEvent(42);

        // Act
        await publisher.PublishAsync(@event);

        // Give the background processor time to consume the event
        await Task.Delay(500);
        await cts.CancelAsync();

        // Assert
        handler.ReceivedEvents.Should().HaveCount(1);
        handler.ReceivedEvents[0].QuestionSetId.Should().Be(42);
    }

    [Fact]
    public async Task PublishAsync_ShouldDeliverMultipleEventsInOrder()
    {
        // Arrange
        var handler = new TestQuestionSetDeletedHandler();
        var services = BuildServiceProvider(handler);
        var publisher = services.GetRequiredService<IIntegrationEventPublisher>();
        var hostedService = services.GetServices<IHostedService>()
            .OfType<IntegrationEventProcessor>().Single();

        using var cts = new CancellationTokenSource();
        var processorTask = hostedService.StartAsync(cts.Token);

        // Act
        await publisher.PublishAsync(new QuestionSetDeletedEvent(1));
        await publisher.PublishAsync(new QuestionSetDeletedEvent(2));
        await publisher.PublishAsync(new QuestionSetDeletedEvent(3));

        await Task.Delay(500);
        await cts.CancelAsync();

        // Assert
        handler.ReceivedEvents.Should().HaveCount(3);
        handler.ReceivedEvents.Select(e => e.QuestionSetId).Should().ContainInOrder(1, 2, 3);
    }

    [Fact]
    public async Task PublishAsync_ShouldDeliverToMultipleHandlersForSameEvent()
    {
        // Arrange
        var handler1 = new TestQuestionSetDeletedHandler();
        var handler2 = new TestQuestionSetDeletedHandler();

        var channel = new IntegrationEventChannel();
        var services = new ServiceCollection()
            .AddSingleton(channel)
            .AddScoped<IIntegrationEventPublisher, IntegrationEventPublisher>()
            .AddScoped<IIntegrationEventHandler<QuestionSetDeletedEvent>>(_ => handler1)
            .AddScoped<IIntegrationEventHandler<QuestionSetDeletedEvent>>(_ => handler2)
            .AddLogging(b => b.AddConsole())
            .BuildServiceProvider();

        var publisher = services.GetRequiredService<IIntegrationEventPublisher>();
        var processor = new IntegrationEventProcessor(
            channel,
            services.GetRequiredService<IServiceScopeFactory>(),
            services.GetRequiredService<ILogger<IntegrationEventProcessor>>());

        using var cts = new CancellationTokenSource();
        await processor.StartAsync(cts.Token);

        // Act
        await publisher.PublishAsync(new QuestionSetDeletedEvent(99));
        await Task.Delay(500);
        await cts.CancelAsync();

        // Assert
        handler1.ReceivedEvents.Should().HaveCount(1);
        handler2.ReceivedEvents.Should().HaveCount(1);
    }

    [Fact]
    public async Task PublishAsync_HandlerException_ShouldNotStopProcessing()
    {
        // Arrange
        var faultyHandler = new FaultyHandler();
        var goodHandler = new TestQuestionSetDeletedHandler();

        var channel = new IntegrationEventChannel();
        var services = new ServiceCollection()
            .AddSingleton(channel)
            .AddScoped<IIntegrationEventPublisher, IntegrationEventPublisher>()
            .AddScoped<IIntegrationEventHandler<QuestionSetDeletedEvent>>(_ => faultyHandler)
            .AddScoped<IIntegrationEventHandler<QuestionSetDeletedEvent>>(_ => goodHandler)
            .AddLogging(b => b.AddConsole())
            .BuildServiceProvider();

        var publisher = services.GetRequiredService<IIntegrationEventPublisher>();
        var processor = new IntegrationEventProcessor(
            channel,
            services.GetRequiredService<IServiceScopeFactory>(),
            services.GetRequiredService<ILogger<IntegrationEventProcessor>>());

        using var cts = new CancellationTokenSource();
        await processor.StartAsync(cts.Token);

        // Act - first event triggers faulty handler, second should still be processed
        await publisher.PublishAsync(new QuestionSetDeletedEvent(1));
        await publisher.PublishAsync(new QuestionSetDeletedEvent(2));
        await Task.Delay(500);
        await cts.CancelAsync();

        // Assert - good handler should have received both events despite faulty handler throwing
        goodHandler.ReceivedEvents.Should().HaveCount(2);
    }

    [Fact]
    public async Task PublishAsync_DifferentEventTypes_ShouldRouteToCorrectHandlers()
    {
        // Arrange
        var questionSetHandler = new TestQuestionSetDeletedHandler();
        var userDeletedHandler = new TestUserDeletedHandler();

        var channel = new IntegrationEventChannel();
        var services = new ServiceCollection()
            .AddSingleton(channel)
            .AddScoped<IIntegrationEventPublisher, IntegrationEventPublisher>()
            .AddScoped<IIntegrationEventHandler<QuestionSetDeletedEvent>>(_ => questionSetHandler)
            .AddScoped<IIntegrationEventHandler<UserDeletedEvent>>(_ => userDeletedHandler)
            .AddLogging(b => b.AddConsole())
            .BuildServiceProvider();

        var publisher = services.GetRequiredService<IIntegrationEventPublisher>();
        var processor = new IntegrationEventProcessor(
            channel,
            services.GetRequiredService<IServiceScopeFactory>(),
            services.GetRequiredService<ILogger<IntegrationEventProcessor>>());

        using var cts = new CancellationTokenSource();
        await processor.StartAsync(cts.Token);

        var userId = Guid.NewGuid();

        // Act
        await publisher.PublishAsync(new QuestionSetDeletedEvent(10));
        await publisher.PublishAsync(new UserDeletedEvent(userId));
        await Task.Delay(500);
        await cts.CancelAsync();

        // Assert
        questionSetHandler.ReceivedEvents.Should().HaveCount(1);
        questionSetHandler.ReceivedEvents[0].QuestionSetId.Should().Be(10);

        userDeletedHandler.ReceivedEvents.Should().HaveCount(1);
        userDeletedHandler.ReceivedEvents[0].UserId.Should().Be(userId);
    }

    #region Test Helpers

    private static ServiceProvider BuildServiceProvider(TestQuestionSetDeletedHandler handler)
    {
        var channel = new IntegrationEventChannel();
        return new ServiceCollection()
            .AddSingleton(channel)
            .AddScoped<IIntegrationEventPublisher, IntegrationEventPublisher>()
            .AddScoped<IIntegrationEventHandler<QuestionSetDeletedEvent>>(_ => handler)
            .AddLogging(b => b.AddConsole())
            .AddSingleton<IHostedService>(sp => new IntegrationEventProcessor(
                sp.GetRequiredService<IntegrationEventChannel>(),
                sp.GetRequiredService<IServiceScopeFactory>(),
                sp.GetRequiredService<ILogger<IntegrationEventProcessor>>()))
            .BuildServiceProvider();
    }

    private sealed class TestQuestionSetDeletedHandler : IIntegrationEventHandler<QuestionSetDeletedEvent>
    {
        public List<QuestionSetDeletedEvent> ReceivedEvents { get; } = [];

        public Task HandleAsync(QuestionSetDeletedEvent @event, CancellationToken cancellationToken = default)
        {
            ReceivedEvents.Add(@event);
            return Task.CompletedTask;
        }
    }

    private sealed class TestUserDeletedHandler : IIntegrationEventHandler<UserDeletedEvent>
    {
        public List<UserDeletedEvent> ReceivedEvents { get; } = [];

        public Task HandleAsync(UserDeletedEvent @event, CancellationToken cancellationToken = default)
        {
            ReceivedEvents.Add(@event);
            return Task.CompletedTask;
        }
    }

    private sealed class FaultyHandler : IIntegrationEventHandler<QuestionSetDeletedEvent>
    {
        public Task HandleAsync(QuestionSetDeletedEvent @event, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Simulated handler failure");
        }
    }

    #endregion
}
