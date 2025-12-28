
using Bus.Shared.Events;
using Microsoft.Extensions.Logging;
using Rabbitmq.Api.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace Rabbitmq.Api.Consumer
{
    public class UserCreatedEventConsumer2(IBusService busService) : BackgroundService
    {
        private IChannel _channel; // İletişim kanalı (channel) nesnesi.
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = await busService.CreateChannel(); // Yeni bir iletişim kanalı (channel) oluşturur.

            await _channel!.QueueDeclareAsync(
                queue: "api-user.created.event-queue-2", // Kuyruğun adı
                durable: true, // Kuyruğun kalıcı olup olmadığı
                exclusive: false, // Kuyruğun yalnızca bu bağlantı tarafından kullanılıp kullanılmayacağı
                autoDelete: false, // Kuyruğun otomatik olarak silinip silinmeyeceği
                arguments: null, // Ek argümanlar
                cancellationToken: cancellationToken // İptal token'ı
            );

            await _channel.QueueBindAsync(
                queue: "api-user.created.event-queue-2", // Kuyruğun adı
                exchange: "user.created.event-exchange", // Bağlanacak exchange'in adı
                routingKey: string.Empty, // Yönlendirme anahtarı (fanout exchange için boş bırakılır)
                arguments: null, // Ek argümanlar
                cancellationToken: cancellationToken // İptal token'ı
            );

            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _channel.DisposeAsync(); // Kanalı kapatır ve kaynakları serbest bırakır.

            await base.StopAsync(cancellationToken); // Temel sınıfın durdurma işlemini çağırır.
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel!); // Asenkron olay tabanlı tüketici oluşturur.

            consumer.ReceivedAsync += Consumer_ReceivedAsync; // Mesaj alındığında tetiklenecek olay işleyicisini ekler.

            await _channel!.BasicConsumeAsync( // Kuyruktan mesaj tüketmeye başlar.
                queue: "api-user.created.event-queue-2", // Tüketilecek kuyruğun adı
                autoAck: true, // Mesajların otomatik olarak onaylanıp onaylanmayacağı
                consumerTag: "api-user.created.event-queue", // Tüketici etiketi
                consumer: consumer, // Tüketici nesnesi
                cancellationToken: stoppingToken // İptal token'ı
            );
        }

        private async Task Consumer_ReceivedAsync(object sender, BasicDeliverEventArgs @event) // Mesaj alındığında çağrılan event handler metodu.
        {
            //sender: Mesajı gönderen nesne.
            //@event: Mesajla ilgili teslimat bilgilerini içeren argümanlar.

            string eventAsJsonString = System.Text.Encoding.UTF8.GetString(bytes: @event.Body.ToArray()); // Mesajın gövdesini UTF-8 string'e dönüştürür.

            var userCreatedEvent = JsonSerializer.Deserialize<UserCreatedEvent>(eventAsJsonString); // JSON string'ini UserCreatedEvent nesnesine deserialize eder.

            Console.WriteLine(
                $"User Created Event Consumed in API Service 2: " +
                $"{userCreatedEvent?.UserName} - {userCreatedEvent?.Email}"
            ); // Konsola kullanıcı adı ve email bilgilerini yazdırır.

            await Task.CompletedTask; // Asenkron metot için tamamlanma bildirimi döner.
        }
    }
}
