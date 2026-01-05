using MassTransit;
using Order.API.Models;
using Shared.Events;

namespace Order.API.Consumers;

public class StockNotReservedEventConsumer : IConsumer<StockNotReservedEvent>
{
    private readonly OrderApiDbContext _dbContext;
    public StockNotReservedEventConsumer(OrderApiDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task Consume(ConsumeContext<StockNotReservedEvent> context)
    {
        Models.Entities.Order order = _dbContext.Orders.Find(context.Message.OrderId);
        order.OrderStatus = Models.Enums.OrderStatus.Failed;
        _dbContext.Orders.Update(order);
        await _dbContext.SaveChangesAsync();
    }
}