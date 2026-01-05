using Order.API.Models.Enums;

namespace Order.API.ViewModels
{
    public class CreateOrder
    {
        public Guid BuyerId { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; }
    }

    public class OrderItemViewModel
    {
        public string ProductId { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }
    }
}