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
    private readonly IPublishEndpoint _publishEndpoint;

    public OrderCreatedEventConsumer(MongoDbService mongoDbService, ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint)
    {
        _stockCollection = mongoDbService.GetCollection<Models.Entities.Stock>();
        _sendEndpointProvider = sendEndpointProvider;
        _publishEndpoint = publishEndpoint;
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
            StockNotReservedEvent stockNotReservedEvent = new()
            {
                OrderId = context.Message.OrderId,
                BuyerId = context.Message.BuyerId,
                Message = "Stock not sufficient"
            };

            await _publishEndpoint.Publish(stockNotReservedEvent);
        }
        await Task.CompletedTask;
    }
}