
using Bus.Shared;
using Bus.Shared.Events;
using Rabbitmq.Api.Repositories;
using RabbitMQ.Client;

namespace Rabbitmq.Api.Consumer
{
    public class UserCreatedEventOutboxConsumer(IServiceProvider serviceProvider, IBusService busService) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = serviceProvider.CreateAsyncScope(); // Yeni bir hizmet kapsamı oluşturur.
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>(); // AppDbContext hizmetini alır.
                var pendingOutboxEvents = dbContext.OutBoxes
                    .Where(o => !o.IsSent && o.EventType == EventType.UserCreated).Take(100).ToList(); // Gönderilmemiş outbox olaylarını alır.

                foreach (var outboxEvent in pendingOutboxEvents) // Her bir outbox olayı için
                {
                    var headers = new Dictionary<string, object> // Mesaj başlıklarını tutan sözlük.
                    {
                        { "idempotency-key", outboxEvent.IdempotencyKey }, // Mesaj kimliğini başlıklara ekler.
                        { "event-type", outboxEvent.EventType } // Mesaj kimliğini başlıklara ekler.
                    };

                    var userCreatedEvent = System.Text.Json.JsonSerializer.Deserialize<UserCreatedEvent>(outboxEvent.EventData!); // Olay verisini UserCreatedEvent nesnesine dönüştürür.

                    busService.PublishWithAck(userCreatedEvent, headers); // Olayı RabbitMQ'ya yayınlar.
                    outboxEvent.IsSent = true; // Olayın gönderildiğini işaretler.
                }
                 await dbContext.SaveChangesAsync(stoppingToken);
                // Belirli bir süre bekleyin (örneğin, 5 saniye) ve ardından tekrar kontrol edin.
                Task.Delay(TimeSpan.FromSeconds(5), stoppingToken).Wait(stoppingToken);
            }
        }
    }
}
