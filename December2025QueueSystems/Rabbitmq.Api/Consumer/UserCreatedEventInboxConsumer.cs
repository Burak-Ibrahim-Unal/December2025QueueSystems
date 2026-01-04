
using Bus.Shared;
using Bus.Shared.Events;
using Rabbitmq.Api.Repositories;
using RabbitMQ.Client;

namespace Rabbitmq.Api.Consumer
{
    public class UserCreatedEventInboxConsumer(IServiceProvider serviceProvider, IBusService busService) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = serviceProvider.CreateAsyncScope(); // Yeni bir hizmet kapsamı oluşturur.
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>(); // AppDbContext hizmetini alır.
                var pendingInboxEvents = dbContext.Inboxes
                    .Where(i => !i.IsProcess && i.EventType == EventType.UserCreated).Take(100).ToList(); // Gönderilmemiş inbox olaylarını alır.

                foreach (var inboxEvent in pendingInboxEvents) // Her bir inbox olayı için
                {
                    UserCreatedEvent userCreatedEvent = System.Text.Json.JsonSerializer.Deserialize<UserCreatedEvent>(inboxEvent.EventData!)!; // Olay verisini UserCreatedEvent nesnesine dönüştürür.

                    var discount  = new Discount // Yeni bir Discount nesnesi oluşturur.
                    {
                        Rate = 0.1,
                        Id = userCreatedEvent.UserId,
                        IsUsed = false
                    };

                    await dbContext.Discounts.AddAsync(discount, stoppingToken); // Yeni bir Discount kaydı ekler.
                    inboxEvent.IsProcess = true; // Olayın işlendiğini işaretler.

                    await dbContext.SaveChangesAsync(stoppingToken);
                }

            }
        }
    }
}
