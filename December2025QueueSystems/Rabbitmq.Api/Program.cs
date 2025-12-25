using Bus.Shared.Options;
using Microsoft.Extensions.Options;
using Rabbitmq.Api.Services;

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

builder.Services.AddSingleton<IBusService, RabbitMqBusService>(sp =>
{
    ServiceBusOption serviceBusOptions = sp.GetRequiredService<ServiceBusOption>();

    var rabbitMqBus = new RabbitMqBusService(serviceBusOptions);
    rabbitMqBus.Init().Wait();

    return rabbitMqBus;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
