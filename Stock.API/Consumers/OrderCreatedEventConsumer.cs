using MassTransit;
using MongoDB.Driver;
using Shared;
using Shared.Events;
using Shared.Messages;
using Stock.API.Services;

namespace Stock.API.Consumers;

public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
{
    IMongoCollection<Models.Entities.Stock> _stockCollection;
    private readonly ISendEndpointProvider _sendEndpointProvider;

    public OrderCreatedEventConsumer(MongoDbService mongoDbService, ISendEndpointProvider sendEndpointProvider)
    {
        _stockCollection = mongoDbService.GetCollection<Models.Entities.Stock>();
        _sendEndpointProvider = sendEndpointProvider;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        List<bool> stockResult = new();
        foreach (OrderItemMessage orderItem in context.Message.OrderItems)
        {
            stockResult.Add((await _stockCollection.FindAsync(s => s.ProductId == orderItem.ProductId && s.Count >= orderItem.Count)).Any());

        }
        if (stockResult.TrueForAll(sr => sr.Equals(true)))
        {
            foreach (OrderItemMessage orderItem in context.Message.OrderItems)
            {
                var stock = await _stockCollection.FindAsync(s => s.ProductId == orderItem.ProductId);
                var existStock = stock.FirstOrDefault();
                existStock.Count -= orderItem.Count;
                await _stockCollection.ReplaceOneAsync(s => s.Id == existStock.Id, existStock);
            }
            StockReservedEvent stockReservedEvent = new()
            {
                BuyerId = context.Message.BuyerId,
                OrderId = context.Message.OrderId,
                TotalPrice = context.Message.TotalPrice
            };

            ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMqSettings.PaymentServiceHostAddress}"));
            await sendEndpoint.Send(stockReservedEvent);
        }
        else
        {
            // stok yetersiz gibi işlemler
        }
        await Task.CompletedTask;
    }
}