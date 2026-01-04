using Bus.Shared.Events;

namespace Rabbitmq.Api.Repositories
{
    public class OutBox
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public EventType EventType { get; set; }
        public string EventData { get; set; } = string.Empty;
        public bool IsSent { get; set; }
    }
}
