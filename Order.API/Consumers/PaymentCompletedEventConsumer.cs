using MassTransit;
using Order.API.Models;
using Shared.Events;

namespace Order.API.Consumers;

public class PaymentCompletedEventConsumer : IConsumer<PaymentCompletedEvent>
{
    private readonly OrderApiDbContext _orderApiDbContext;

    public PaymentCompletedEventConsumer(OrderApiDbContext orderApiDbContext)
    {
        _orderApiDbContext = orderApiDbContext;
    }

    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        var order = _orderApiDbContext.Orders.FirstOrDefault(o => o.OrderId == context.Message.OrderId);
        order.OrderStatus = Models.Enums.OrderStatus.Completed;
        _orderApiDbContext.Orders.Update(order);
        await _orderApiDbContext.SaveChangesAsync();
    }
}