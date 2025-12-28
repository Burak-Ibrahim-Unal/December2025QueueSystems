using Bus.Shared.Events;
using RabbitMQ.Client;

namespace Rabbitmq.Api.Services
{
    public interface IBusService
    {
        Task Publish<T>(T message) where T : BaseEvent;
        Task<IChannel> CreateChannel();
    }
}
