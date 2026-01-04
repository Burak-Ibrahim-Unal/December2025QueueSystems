
using Bus.Shared;
using Bus.Shared.Events;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace Rabbitmq.Api.Consumer
{
    public class UserCreatedEventConsumer(IBusService busService) : BackgroundService
    {
        private IChannel _channel; // İletişim kanalı (channel) nesnesi.
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var exchangeName = RabbitMqBusService.GetExchangeName<UserCreatedEvent>();

            _channel = await busService.CreateChannel(); // Yeni bir iletişim kanalı (channel) oluşturur.


            await _channel.BasicQosAsync(
                prefetchSize: 0, // Önceden alınan mesajların toplam boyutu (byte cinsinden). 0, sınırsız anlamına gelir.
                prefetchCount: 500, // Aynı anda işlenebilecek maksimum mesaj sayısı. 500 mesaj işleneceği anlamına gelir.Bir mesaj işlendiğinde 500'e tamamlayacak şekilde yeni bir mesaj alınır.
                global: false // Ayarın tüm kanal için mi yoksa sadece bu tüketici için mi geçerli olduğunu belirtir.
            );

            await _channel!.QueueDeclareAsync(
                queue: "api-user.created.event-queue", // Kuyruğun adı
                durable: true, // Kuyruğun kalıcı olup olmadığı
                exclusive: false, // Kuyruğun yalnızca bu bağlantı tarafından kullanılıp kullanılmayacağı
                autoDelete: false, // Kuyruğun otomatik olarak silinip silinmeyeceği
                arguments: null, // Ek argümanlar
                cancellationToken: cancellationToken // İptal token'ı
            );

            await _channel.QueueBindAsync(
                queue: "api-user.created.event-queue", // Kuyruğun adı
                exchange: exchangeName, // Bağlanacak exchange'in adı
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
                queue: "api-user.created.event-queue", // Tüketilecek kuyruğun adı
                autoAck: false, // Mesajların otomatik olarak onaylanıp onaylanmayacağı.True olduğunda mesaj exhange'e iletildikten sonra silinir.
                consumerTag: "api-user.created.event-queue", // Tüketici etiketi
                consumer: consumer, // Tüketici nesnesi
                cancellationToken: stoppingToken // İptal token'ı
            );
        }

        private async Task Consumer_ReceivedAsync(object sender, BasicDeliverEventArgs args) // Mesaj alındığında çağrılan event handler metodu.
        {
            //inbox + idempotency



            //sender: Mesajı gönderen nesne.
            //@args: Mesajla ilgili teslimat bilgilerini içeren argümanlar.

            string eventAsJsonString = System.Text.Encoding.UTF8.GetString(bytes: args.Body.ToArray()); // Mesajın gövdesini UTF-8 string'e dönüştürür.

            var userCreatedEvent = JsonSerializer.Deserialize<UserCreatedEvent>(eventAsJsonString); // JSON string'ini UserCreatedEvent nesnesine deserialize eder.

            Console.WriteLine(
                $"User Created Event Consumed in API Service: " +
                $"{userCreatedEvent?.UserName} - {userCreatedEvent?.Email}"
            ); // Konsola kullanıcı adı ve email bilgilerini yazdırır.

            await _channel.BasicAckAsync( // Mesajın başarıyla işlendiğini bildirir.
                deliveryTag: args.DeliveryTag, // Teslimat etiketi
                multiple: false // Aynı anda birden fazla mesajın onaylanıp onaylanmayacağı
            );
        }
    }
}
