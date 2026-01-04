using Bus.Shared.Events;
using Bus.Shared.Options;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Bus.Shared
{
    public class RabbitMqBusService(ServiceBusOption serviceBusOption) : IBusService
    {
        private IChannel? _channelWithAck; // Yayıncı onaylı iletişim kanalı (channel) nesnesi.
        private IChannel? _channelWithNoAck; // Yayıncı onaysız iletişim kanalı (channel) nesnesi.
        public static Dictionary<object, string> ExchangeList = new(); // Oluşturulan exchange'leri saklamak için kullanılan statik sözlük.
        private IConnection? _connection; // RabbitMQ sunucusuna olan tcp bağlantısını tutan değişken.

        static RabbitMqBusService()
        {
            ExchangeList.Add(typeof(UserCreatedEvent), GetExchangeName<UserCreatedEvent>()); // UserCreatedEvent türü için exchange adını sözlüğe ekler.
        }

        public async Task Init() // Servis başladığında bağlantıyı kurmak için çağrılan asenkron başlatma metodu.
        {
            var connectionFactory = new ConnectionFactory // Bağlantı oluşturmak için gerekli fabrika nesnesinin örneği.
            {
                Uri = new Uri(serviceBusOption.RabbitMqConnectionString) // Bağlantı string'ini (URL) ayarlardan alıp uri formatında atar.
            };

            _connection = await connectionFactory.CreateConnectionAsync(); // Asenkron olarak RabbitMQ sunucusuna bağlanır ve bağlantıyı saklar.

            _channelWithAck = await _connection!.CreateChannelAsync(
                new CreateChannelOptions(
                    publisherConfirmationsEnabled: true, // Yayıncı onaylarının etkinleştirilmesini sağlar.
                    publisherConfirmationTrackingEnabled: true // Yayıncı onay takibinin etkinleştirilmesini sağlar.
                )
            );

            _channelWithNoAck = await _connection!.CreateChannelAsync();
        }

        public async Task PublishWithNoAck<T>(T message) where T : BaseEvent // Generic bir olay yayınlama metodu; T, BaseEvent'ten türetilmiş olmalı.
        {
            //Act No, No Retry
            //At-Most once
            //fire and forget
            string exchangeName = GetExchangeName<T>(); // Olay türüne göre exchange adı oluşturur.

            await _channelWithNoAck!.ExchangeDeclareAsync( // Exchange'i declare eder.
                exchange: exchangeName, // Exchange'in adını belirler.
                type: ExchangeType.Fanout, // Exchange tipini fanout (tüm kuyruklaara dağıt) olarak belirler.
                durable: true, //   Exchange'in kalıcı olmasını (sunucu kapansa bile silinmemesini) sağlar.
                autoDelete: false, // Exchange kullanılmadığında otomatik silinmesini engeller.
                arguments: null // Ek argümanlar için null değerini atar.
            );

            string eventAsJsonData = JsonSerializer.Serialize(message); // Olay nesnesini JSON formatına serileştirir.

            byte[] body = System.Text.Encoding.UTF8.GetBytes(eventAsJsonData); // JSON verisini UTF-8 byte dizisine dönüştürür.

            var properties = new BasicProperties // Mesajın özelliklerini belirlemek için kullanılan nesne.
            {
                Persistent = true // Mesajın kalıcı olarak diskte saklar (sunucu kapansa bile silinmemesini sağlar).
            };

            await _channelWithNoAck.BasicPublishAsync(
                exchange: exchangeName, // Mesajın gönderileceği exchange adı (T tipine göre dinamik oluşturulur).
                routingKey: string.Empty, // Fanout exchange için yönlendirme anahtarı boş bırakılır.
                mandatory: false, // Mesajın teslim edilememesi durumunda iade edilmesini engeller.
                properties, // Mesajın özelliklerini belirten nesne.
                body // Mesajın içeriği (byte dizisi).
            );
        }

        public async Task PublishWithAck<T>(T message , Dictionary<string,object>? headers = null) where T : BaseEvent // Generic bir olay yayınlama metodu; T, BaseEvent'ten türetilmiş olmalı.
        {
            string exchangeName = GetExchangeName<T>(); // Olay türüne göre exchange adı oluşturur.

            await _channelWithAck!.ExchangeDeclareAsync( // Exchange'i declare eder.
                exchange: exchangeName, // Exchange'in adını belirler.
                type: ExchangeType.Fanout, // Exchange tipini fanout (tüm kuyruklaara dağıt) olarak belirler.
                durable: true, //   Exchange'in kalıcı olmasını (sunucu kapansa bile silinmemesini) sağlar.
                autoDelete: false, // Exchange kullanılmadığında otomatik silinmesini engeller.
                arguments: null); // Ek argümanlar için null değerini atar.

            string eventAsJsonData = JsonSerializer.Serialize(message); // Olay nesnesini JSON formatına serileştirir.

            byte[] body = System.Text.Encoding.UTF8.GetBytes(eventAsJsonData); // JSON verisini UTF-8 byte dizisine dönüştürür.

            var properties = new BasicProperties // Mesajın özelliklerini belirlemek için kullanılan nesne.
            {
                Persistent = true, // Mesajın kalıcı olarak diskte saklar (sunucu kapansa bile silinmemesini sağlar).
            };

            if (headers != null) // Başlıklar sağlanmışsa
            {
                properties.Headers = headers; // Eğer başlıklar sağlanmışsa, mesaj özelliklerine ekler.
            }

            const int maxRetries = 3; // Maksimum yeniden deneme sayısı.
            int attempt = 0; // Mevcut deneme sayısı.

            while (attempt < maxRetries)
            {
                try
                {
                    attempt++;

                    await _channelWithAck!.BasicPublishAsync( // Mesajı exchange'e yayınlar.
                        exchange: exchangeName, // Mesajın gönderileceği exchange adı (T tipine göre dinamik oluşturulur).
                        routingKey: string.Empty, // Fanout exchange için yönlendirme anahtarı boş bırakılır.
                        mandatory: true, // Mesajın teslim edilememesi durumunda iade edilmesini sağlar.
                        properties, // Mesajın özelliklerini belirten nesne.
                        body // Mesajın içeriği (byte dizisi).
                    );

                    break;
                }
                catch (Exception) when (attempt < maxRetries) // Hata oluşursa ve maksimum deneme sayısına ulaşılmadıysa
                {
                    await Task.Delay(
                        TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)) // Üstel geri çekilme süresi (2^(attempt-1) saniye).
                    );
                }
            }
        }

        public Task<IChannel> CreateChannel()
        {
            return _connection.CreateChannelAsync(); // Mevcut bağlantı üzerinden yeni bir iletişim kanalı (channel) açar ve döner.
        }

        // Exchange ismini oluşturan helper metod
        public static string GetExchangeName<T>()
        {
            return $"{typeof(T).Name.ToLower()}-exchange";
        }

        public async Task CreateExchanges()
        {


            IChannel channel = await _connection!.CreateChannelAsync();
            foreach (KeyValuePair<object, string> exchange in ExchangeList)
            {
                await channel.ExchangeDeclareAsync(exchange.Value, ExchangeType.Fanout, true, false, null);
            }
            await channel.DisposeAsync();
        }
    }
}
