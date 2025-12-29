using Bus.Shared;
using Bus.Shared.Options;
using Microsoft.Extensions.Options;
using Rabbitmq.Api.Consumer;
using TestEducation.Examples;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<ServiceBusOption>(
    builder.Configuration.GetSection(nameof(ServiceBusOption)));

builder.Services.AddSingleton<ServiceBusOption>(sp =>
{
    var optionsServiceBus = sp.GetRequiredService<IOptions<ServiceBusOption>>();
    return optionsServiceBus.Value;
});

builder.Services.AddScoped<UserService>();
builder.Services.AddSingleton<IBusService, RabbitMqBusService>(sp =>
{
    ServiceBusOption serviceBusOptions = sp.GetRequiredService<ServiceBusOption>();

    var rabbitMqBus = new RabbitMqBusService(serviceBusOptions);
    rabbitMqBus.Init().Wait();

    return rabbitMqBus;
});

builder.Services.AddHostedService<UserCreatedEventConsumer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapPost("/api/users", async (UserService userService) =>
{
    await userService.CreateUser();
    Results.Ok("User created and event published.");
})
.WithName("Create User");

app.Run();
