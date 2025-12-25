using Bus.Shared.Events;
using Bus.Shared.Options;
using RabbitMQ.Client;

namespace Rabbitmq.Api.Services
{
    public class RabbitMqBusService(ServiceBusOption serviceBusOption) : IBusService
    {
        private IConnection? _connection;
        public async Task Init()
        {
            var connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(serviceBusOption.RabbitMqConnectionString)
            };

            _connection = await connectionFactory.CreateConnectionAsync();
        }

        public async Task Publish<T>(T message) where T : BaseEvent
        {

            var channel = await _connection.CreateChannelAsync();
        }
    }
}
