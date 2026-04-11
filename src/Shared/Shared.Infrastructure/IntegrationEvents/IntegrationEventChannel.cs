using System.Threading.Channels;
using Shared.Core.IntegrationEvents;

namespace Shared.Infrastructure.IntegrationEvents;

public sealed class IntegrationEventChannel
{
    private readonly Channel<IIntegrationEvent> _channel;

    public IntegrationEventChannel()
    {
        _channel = Channel.CreateBounded<IIntegrationEvent>(new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });
    }

    public ChannelWriter<IIntegrationEvent> Writer => _channel.Writer;
    public ChannelReader<IIntegrationEvent> Reader => _channel.Reader;
}
