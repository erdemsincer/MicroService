using MassTransit;
using Microsoft.OpenApi.Writers;
using MongoDB.Driver;
using Shared;
using Stock.API.Consumers;
using Stock.API.Services;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OrderCreatedEventConsumer>();
    configurator.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.ReceiveEndpoint(RabbitMqSettings.StockServiceHostAddress, e =>
        {
            e.ConfigureConsumer<OrderCreatedEventConsumer>(context);
        });
    });
});

builder.Services.AddSingleton<MongoDbService>();

using IServiceScope scope = builder.Services.BuildServiceProvider().CreateScope();
MongoDbService mongoDbService = scope.ServiceProvider.GetService<MongoDbService>();
var collection = mongoDbService.GetCollection<Stock.API.Models.Entities.Stock>();
if (!collection.FindSync(s => true).Any())
{
    await collection.InsertManyAsync(new[]
    {
        new Stock.API.Models.Entities.Stock
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid().ToString(),
            Count = 100
        },
        new Stock.API.Models.Entities.Stock
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid().ToString(),
            Count = 200
        },
        new Stock.API.Models.Entities.Stock
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid().ToString(),
            Count = 300
        }
    });
}


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.UseAuthorization();

app.MapControllers();

app.Run();


