namespace Payment.API.Consumers;

using MassTransit;
using Shared.Events;

public class StockReservedEventConsumer : IConsumer<StockReservedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public StockReservedEventConsumer(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<StockReservedEvent> context)
    {
        if (true)
        {
            PaymentCompletedEvent paymentCompletedEvent = new()
            {
                OrderId = context.Message.OrderId,
            };
            await _publishEndpoint.Publish(paymentCompletedEvent);
        }
        else
        {
            PaymentFailedEvent paymentFailedEvent = new()
            {
                OrderId = context.Message.OrderId,
                Message = "Payment failed due to insufficient balance"
            };
            await _publishEndpoint.Publish(paymentFailedEvent);
        }

    }
}


