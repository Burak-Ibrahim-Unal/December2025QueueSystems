using Bus.Shared.Events;

namespace Rabbitmq.Api.Services
{
    public interface IBusService
    {
        Task Publish<T>(T message) where T : BaseEvent;
    }
}
