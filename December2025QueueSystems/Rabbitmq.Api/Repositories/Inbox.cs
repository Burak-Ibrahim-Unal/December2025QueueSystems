using Bus.Shared.Events;

namespace Rabbitmq.Api.Repositories
{

    public partial class AppDbContext
    {
        public class Inbox
        {
            public int Id { get; set; }

            public EventType EventType { get; set; }

            public string EventData { get; set; } = string.Empty;

            public DateTime CreatedAt { get; set; }

            public bool IsProcess { get; set; }
        }
    }
}
