using MassTransit;
using Order.API.Models;
using Shared.Events;

namespace Order.API.Consumers;

public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
{
    private readonly OrderApiDbContext _orderDbContext;
    public PaymentFailedEventConsumer(OrderApiDbContext orderDbContext)
    {
        _orderDbContext = orderDbContext;
    }
    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        var order = _orderDbContext.Orders.Find(context.Message.OrderId);

        order.OrderStatus = Models.Enums.OrderStatus.Failed;
        _orderDbContext.Orders.Update(order);
        await _orderDbContext.SaveChangesAsync();
    }
}