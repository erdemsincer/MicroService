namespace Shared.Events;

public class PaymentFailedEvent
{
    public Guid OrderId { get; set; }
    public string Message { get; set; }
}