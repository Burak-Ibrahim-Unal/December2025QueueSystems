// See https://aka.ms/new-console-template for more information


using RabbitMQ.Client;
using RabbitMQ.Client.Events;

Console.WriteLine("Dead Letter Exchange");

const string mainExchange = "main.exchange-direct";
const string mainQueue = "main-direct.queue";

const string deadLetterExchange = "dead.letter.direct.exchange";
const string deadLetterQueue = "dead.letter.direct.queue";


var connectionFactory = new ConnectionFactory
{
    Uri = new Uri("amqps://oypfspzz:jcWNYrihOkCeXeWy02f_oYhmeyO2MRBm@leopard.lmq.cloudamqp.com/oypfspzz")
};

using var connection = await connectionFactory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

// Ensure clean state to avoid argument mismatch errors
await channel.QueueDeleteAsync(mainQueue);
await channel.QueueDeleteAsync(deadLetterQueue);

//main
await channel.ExchangeDeclareAsync(
    exchange: mainExchange,
    type: ExchangeType.Direct,
    durable: true, // restart durumunda kalıcı olacak
    autoDelete: false, // Bu exhange bağlı kuyruk olmadığında silinmesin
    arguments: null
);

// Dead letter exchange ayarları QueueDeclareAsync'te arguments içinde belirtilmelidir.
var mainQueueArguments = new Dictionary<string, object>()
    {
        {"x-dead-letter-exchange", deadLetterExchange}, // Dead letter exchange adını belirtir.
        {"x-message-ttl", 10000 }, // Mesajların 10 saniye sonra expire olmasını sağlar.
        {"x-dead-letter-routing-key", "route-key-error" }, // Mesajların 10 saniye sonra expire olmasını sağlar.
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
    routingKey: "abc123",
    arguments: null // QueueBindAsync'te dead letter ayarları belirtilmez.
);


// dead letter
await channel.ExchangeDeclareAsync(
    exchange: deadLetterExchange,
    type: ExchangeType.Direct,
    durable: true, // restart durumunda kalıcı olacak
    autoDelete: false, // Bu exhange bağlı kuyruk olmadığında silinmesin
    arguments: null
);


await channel.QueueDeclareAsync(
    queue: deadLetterQueue,
    durable: true, // Kuyruk kalıcı olacak
    exclusive: false, // Kuyruk sadece mevcut bağlantıya özel değil, bağlantı kapansa bile kalacak
    autoDelete: false, // Kuyruk otomatik silinmeyecek
    arguments: null // Dead letter queue arguments ayarları
);

await channel.QueueBindAsync(
    queue: deadLetterQueue,
    exchange: deadLetterExchange,
    routingKey: "route-key-error",
    arguments: null
);


// send messages to main.exchange
var messageBody = "Bir direct exchange test verisidir.";

var properties = new BasicProperties()
{
    Persistent = true,
    //Expiration = "10000"
};

var body = System.Text.Encoding.UTF8.GetBytes(messageBody);
await channel.BasicPublishAsync(
    exchange: mainExchange,
    routingKey: "abc123",
    body: body
);

var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += async (sender, eventArgs) =>
{
    var message = System.Text.Encoding.UTF8.GetString(eventArgs.Body.ToArray());
    Console.WriteLine($"Received Message: {message} | RoutingKey: {eventArgs.RoutingKey}");
    await channel.BasicAckAsync(eventArgs.DeliveryTag, false);
};

await channel.BasicConsumeAsync(queue: mainQueue, autoAck: false, consumer: consumer);

Console.ReadLine();