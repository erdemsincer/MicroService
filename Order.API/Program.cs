using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Consumers;
using Order.API.Models;
using Shared;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrderApiDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SQLServerConnection"));
});

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<PaymentCompletedEventConsumer>();
    configurator.AddConsumer<StockNotReservedEventConsumer>();
    configurator.AddConsumer<PaymentFailedEventConsumer>();
    configurator.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.ReceiveEndpoint(RabbitMqSettings.OrderServiceHostAddress, e =>
        {
            e.ConfigureConsumer<PaymentCompletedEventConsumer>(context);
        });
        cfg.ReceiveEndpoint(RabbitMqSettings.StockNotReservedHostAddress, e =>
        {
            e.ConfigureConsumer<StockNotReservedEventConsumer>(context);
        });
        cfg.ReceiveEndpoint(RabbitMqSettings.PaymentFailedHostAddress, e =>
        {
            e.ConfigureConsumer<PaymentFailedEventConsumer>(context);
        });
    });
});
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

