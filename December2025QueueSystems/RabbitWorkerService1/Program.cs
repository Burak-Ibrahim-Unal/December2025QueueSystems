using Bus.Shared;
using Bus.Shared.Options;
using Microsoft.Extensions.Options;
using Rabbitmq.Api.Consumer;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<UserCreatedEventConsumerWS>();

builder.Services.Configure<ServiceBusOption>(
    builder.Configuration.GetSection(nameof(ServiceBusOption)));

builder.Services.AddSingleton<ServiceBusOption>(sp =>
{
    var optionsServiceBus = sp.GetRequiredService<IOptions<ServiceBusOption>>();
    return optionsServiceBus.Value;
});

builder.Services.AddSingleton<IBusService, RabbitMqBusService>(sp =>
{
    ServiceBusOption serviceBusOptions = sp.GetRequiredService<ServiceBusOption>();

    var rabbitMqBus = new RabbitMqBusService(serviceBusOptions);
    rabbitMqBus.Init().Wait();

    return rabbitMqBus;
});

var host = builder.Build();
host.Run();
