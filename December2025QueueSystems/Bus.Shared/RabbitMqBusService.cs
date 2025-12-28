using Bus.Shared.Events;
using Bus.Shared.Options;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Rabbitmq.Api.Services
{
    public class RabbitMqBusService(ServiceBusOption serviceBusOption) : IBusService
    {
        private IConnection? _connection; // RabbitMQ sunucusuna olan tcp bağlantısını tutan değişken.
        public async Task Init() // Servis başladığında bağlantıyı kurmak için çağrılan asenkron başlatma metodu.
        {
            var connectionFactory = new ConnectionFactory // Bağlantı oluşturmak için gerekli fabrika nesnesinin örneği.
            {
                Uri = new Uri(serviceBusOption.RabbitMqConnectionString) // Bağlantı string'ini (URL) ayarlardan alıp uri formatında atar.
            };

            _connection = await connectionFactory.CreateConnectionAsync(); // Asenkron olarak RabbitMQ sunucusuna bağlanır ve bağlantıyı saklar.

            IChannel channel = await _connection!.CreateChannelAsync(); // Mevcut bağlantı üzerinden yeni bir iletişim kanalı (channel) açar.

            await channel.ExchangeDeclareAsync( // Mesajın yönlendirileceği exchange (değiş tokuş noktası) tanımını yapar.
                exchange: "user.created.event-exchange", // Exchange'in adını belirler.
                type: ExchangeType.Fanout, // Exchange tipini fanout (tüm kuyruklaara dağıt) olarak belirler.
                durable: true, // Exchange'in kalıcı olmasını (sunucu kapansa bile silinmemesini) sağlar.
                autoDelete: false, // Exchange kullanılmadığında otomatik silinmesini engeller.
                arguments: null); // Ek argümanlar için null değerini atar.

            await channel.DisposeAsync(); // Kanalı kapatır ve kaynakları serbest bırakır.
        }

        public async Task PublishWithNoAck<T>(T message) where T : BaseEvent // Generic bir olay yayınlama metodu; T, BaseEvent'ten türetilmiş olmalı.
        {
            //Act No, No Retry
            //At-Most once
            //fire and forget
            IChannel channel = await _connection!.CreateChannelAsync(); // Mevcut bağlantı üzerinden yeni bir iletişim kanalı (channel) açar.

            string exchangeName = $"{typeof(T).Name.ToLower()}-exchange"; // Olay türüne göre exchange adı oluşturur.

            string eventAsJsonData = JsonSerializer.Serialize(message); // Olay nesnesini JSON formatına serileştirir.

            byte[] body = System.Text.Encoding.UTF8.GetBytes(eventAsJsonData); // JSON verisini UTF-8 byte dizisine dönüştürür.

            var properties = new BasicProperties // Mesajın özelliklerini belirlemek için kullanılan nesne.
            {
                Persistent = true // Mesajın kalıcı olarak diskte saklar (sunucu kapansa bile silinmemesini sağlar).
            };

            await channel.BasicPublishAsync(
                exchange: "user.created.event-exchange", // Mesajın gönderileceği exchange adı.
                routingKey: string.Empty, // Fanout exchange için yönlendirme anahtarı boş bırakılır.
                mandatory: false, // Mesajın teslim edilememesi durumunda iade edilmesini engeller.
                properties, // Mesajın özelliklerini belirten nesne.
                body // Mesajın içeriği (byte dizisi).
            );

            await channel.DisposeAsync(); // Kanalı kapatır ve kaynakları serbest bırakır.
        }

        public async Task PublishWithAck<T>(T message) where T : BaseEvent // Generic bir olay yayınlama metodu; T, BaseEvent'ten türetilmiş olmalı.
        {
            //Act Yes, Yes Retry
            //At-Least once
            IChannel channel = await _connection!.CreateChannelAsync(
                new CreateChannelOptions( // Kanal oluşturma seçeneklerini belirten nesne.
                    publisherConfirmationsEnabled: true, // Yayıncı onaylarının etkinleştirilmesini sağlar.
                    publisherConfirmationTrackingEnabled: true // Yayıncı onay takibinin etkinleştirilmesini sağlar.
                )
            ); // Mevcut bağlantı üzerinden yeni bir iletişim kanalı (channel) açar.


            string eventAsJsonData = JsonSerializer.Serialize(message); // Olay nesnesini JSON formatına serileştirir.

            byte[] body = System.Text.Encoding.UTF8.GetBytes(eventAsJsonData); // JSON verisini UTF-8 byte dizisine dönüştürür.

            var properties = new BasicProperties // Mesajın özelliklerini belirlemek için kullanılan nesne.
            {
                Persistent = true // Mesajın kalıcı olarak diskte saklar (sunucu kapansa bile silinmemesini sağlar).
            };

            const int maxRetries = 3;
            int attempt = 0;

            while (attempt < maxRetries)
            {
                try
                {
                    attempt++;


                    await channel.BasicPublishAsync(
                        exchange: "user.created.event-exchange", // Mesajın gönderileceği exchange adı.
                        routingKey: string.Empty, // Fanout exchange için yönlendirme anahtarı boş bırakılır.
                        mandatory: true, // Mesajın teslim edilememesi durumunda iade edilmesini sağlar.
                        properties, // Mesajın özelliklerini belirten nesne.
                        body // Mesajın içeriği (byte dizisi).
                    );

                    break;
                }
                catch (Exception) when (attempt < maxRetries)
                {
                    await Task.Delay(
                        TimeSpan.FromSeconds(Math.Pow(2, attempt - 1))
                    );
                }
            }

            await channel.DisposeAsync(); // Kanalı kapatır ve kaynakları serbest bırakır.
        }

        public Task<IChannel> CreateChannel()
        {
            return _connection.CreateChannelAsync(); // Mevcut bağlantı üzerinden yeni bir iletişim kanalı (channel) açar ve döner.
        }
    }
}
