using Bus.Shared.Events;
using RabbitMQ.Client;

namespace Rabbitmq.Api.Services
{
    public interface IBusService
    {
        Task PublishWithNoAck<T>(T message) where T : BaseEvent;
        Task PublishWithAck<T>(T message) where T : BaseEvent;
        Task<IChannel> CreateChannel();
    }
}
