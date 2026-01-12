// See https://aka.ms/new-console-template for more information
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

//(IConnection connection, IChannel channel) = await DeadLetterScenario1();
(IConnection connection2, IChannel channel2) = await DeadLetterScenario2WithRequeue();

Console.ReadLine();

async Task<(IConnection connection, IChannel channel)> DeadLetterScenario1()
{
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
        {"x-message-ttl", 10000 }, // Mesajların 10 saniye sonra expire olmasını sağlar.
        {"x-delivery-limit",3 } // Bir mesaj en fazla 3 kez yeniden kuyruğa teslim edilmeye çalışılır; 3 denemeden sonra başarısız kabul edilip dead-letter exchange’e yönlendirilir.
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
    return (connection, channel);
}

async Task<(IConnection connection, IChannel channel)> DeadLetterScenario2WithRequeue()
{
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
        {"x-queue-type", "quorum"}, // Kuyruk tipleri
        // "classic" : Eski tip kuyruktur, RAM/disk ağırlıklıdır, çoğu gelişmiş retry/delivery-limit özelliği yoktur.
        // "quorum"  : Raft tabanlı, veri kaybına dayanıklı, retry & delivery-limit destekleyen, güvenilir kuyruk tipidir.
        // "stream"  : Çok yüksek throughput ve event streaming için tasarlanmıştır, log gibi çalışır (Kafka benzeri).Mesajlar asla silinmez.
        {"x-dead-letter-exchange", deadLetterExchange}, // Dead letter exchange adını belirtir.
        {"x-message-ttl", 10000 }, // Mesajların 10 saniye sonra expire olmasını sağlar.
        {"x-delivery-limit", 3}, // 3 kere dener
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

    Console.WriteLine("message is sent to main exhange.it will die after ttl expires time");

    var consumer = new AsyncEventingBasicConsumer(channel);

    consumer.ReceivedAsync += async (sender, eventArgs) =>
    {
        Console.WriteLine($"Message Processing - {eventArgs.DeliveryTag}");

        try
        {
            var receivingMessage = System.Text.Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            await channel.BasicAckAsync(deliveryTag: eventArgs.DeliveryTag, multiple: false);
            Console.WriteLine($"Message acknowledged - {eventArgs.DeliveryTag}");
        }
        catch (Exception)
        {
            await channel.BasicNackAsync(deliveryTag: eventArgs.DeliveryTag, multiple: false, requeue: false);
        }

    };

    await channel.BasicConsumeAsync(queue: mainQueue, autoAck: false, consumer: consumer);
    return (connection, channel);
}





