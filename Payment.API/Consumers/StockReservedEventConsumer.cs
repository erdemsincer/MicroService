namespace Payment.API.Consumers;
using MassTransit;
using Shared.Events;

public class StockReservedEventConsumer : IConsumer<StockReservedEvent>
{
    public async Task Consume(ConsumeContext<StockReservedEvent> context)
    {
        await Task.CompletedTask;
    }

}
