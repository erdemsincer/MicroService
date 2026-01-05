namespace Shared;

public static class RabbitMqSettings
{
    public static string StockServiceHostAddress = "stock-service-rabbitmq";
    public static string PaymentServiceHostAddress = "payment-service-rabbitmq";
    public static string OrderServiceHostAddress = "order-service-rabbitmq";
    public static string StockNotReservedHostAddress = "stock-not-reserved-event-queue";
    public static string PaymentFailedHostAddress = "payment-failed-event-queue";
}