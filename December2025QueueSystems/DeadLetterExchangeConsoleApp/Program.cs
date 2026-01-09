// See https://aka.ms/new-console-template for more information
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

Console.WriteLine("Dead Letter Exchange");

const string mainExchange = "main.exchange";
const string mainQueue = "main.queue";

const string deadLetterExchange = "dead.letter.exchange";
const string deadLetterQueue = "dead.letter.queue";

var connectionFactory = new ConnectionFactory
{
    Uri = new Uri("amqps://oypfspzz:jcWNYrihOkCeXeWy02f_oYhmeyO2MRBm@leopard.lmq.cloudamqp.com/oypfspzz")
};

using var connection = await connectionFactory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

//main
await channel.ExchangeDeclareAsync(
    exchange: mainExchange,
    type: ExchangeType.Fanout,
    durable: true, // restart durumunda kalıcı olacak
    autoDelete: false, // Bu exhange bağlı kuyruk olmadığında silinmesin
    arguments: null
);

// Dead letter exchange ayarları QueueDeclareAsync'te arguments içinde belirtilmelidir.
var mainQueueArguments = new Dictionary<string, object>()
{
    {"x-dead-letter-exchange", deadLetterExchange}, // Dead letter exchange adını belirtir.
    {"x-message-ttl", 10000 } // Mesajların 10 saniye sonra expire olmasını sağlar.
};

await channel.QueueDeclareAsync(
    queue: mainQueue,
    durable: true, // Kuyruk kalıcı olacak
    exclusive: false, // Kuyruk sadece mevcut bağlantıya özel değil, bağlantı kapansa bile kalacak
    autoDelete: false, // Kuyruk otomatik silinmeyecek
    arguments: mainQueueArguments // Dead letter exchange ayarları burada belirtilir.
);

await channel.QueueBindAsync(
    queue: mainQueue,
    exchange: mainExchange,
    routingKey: string.Empty,
    arguments: null // QueueBindAsync'te dead letter ayarları belirtilmez.
);

// dead letter
await channel.ExchangeDeclareAsync(
    exchange: deadLetterExchange,
    type: ExchangeType.Fanout,
    durable: true, // restart durumunda kalıcı olacak
    autoDelete: false, // Bu exhange bağlı kuyruk olmadığında silinmesin
    arguments: null
);

// Dead letter queue için arguments ayarları
var deadLetterQueueArguments = new Dictionary<string, object>()
{
    {"x-dead-letter-exchange", deadLetterExchange }, // Dead letter queue'daki mesajların 24 saat (86400000 ms) sonra expire olmasını sağlar.
    {"x-message-ttl", 20000 } // Dead letter queue'da maksimum 10000 mesaj tutulur, sonrasında eski mesajlar silinir.
};

await channel.QueueDeclareAsync(
    queue: deadLetterQueue,
    durable: true, // Kuyruk kalıcı olacak
    exclusive: false, // Kuyruk sadece mevcut bağlantıya özel değil, bağlantı kapansa bile kalacak
    autoDelete: false, // Kuyruk otomatik silinmeyecek
    arguments: deadLetterQueueArguments // Dead letter queue arguments ayarları
);

await channel.QueueBindAsync(
    queue: deadLetterQueue,
    exchange: deadLetterExchange,
    routingKey: string.Empty,
    arguments: null
);


// send messages to main.exchange
var messageBody = "Bir bir test verisidir.";

var properties = new BasicProperties()
{
    Persistent = true,
    Expiration = "10000"
};

var body = System.Text.Encoding.UTF8.GetBytes(messageBody);
await channel.BasicPublishAsync(
    exchange: mainExchange,
    routingKey: string.Empty,
    body: body
);

var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += async (sender, eventArgs) =>
{
    try
    {
        var receivingMessage = System.Text.Encoding.UTF8.GetString(eventArgs.Body.ToArray());
        Console.WriteLine($"Received message from dead letter queue: {receivingMessage} ");
        await channel.BasicAckAsync(deliveryTag: eventArgs.DeliveryTag, multiple: false);
    }
    catch (Exception)
    {
        await channel.BasicNackAsync(deliveryTag: eventArgs.DeliveryTag, multiple: false, requeue: false);
        throw;
    }

};

await channel.BasicConsumeAsync(queue: deadLetterQueue, autoAck: false, consumer: consumer);