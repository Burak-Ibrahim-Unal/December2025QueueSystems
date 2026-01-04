using Bus.Shared.Events;
using RabbitMQ.Client;

namespace Bus.Shared
{
    public interface IBusService
    {
        Task PublishWithNoAck<T>(T message) where T : BaseEvent;
        Task PublishWithAck<T>(T message, Dictionary<string, object>? headers = null) where T : BaseEvent;
        Task<IChannel> CreateChannel();
    }
}
