using System.ComponentModel.DataAnnotations.Schema;

namespace Order.API.Models.Entities
{
    public class OrderItem
    {
        public Guid OrderItemId { get; set; }

        public string ProductId { get; set; }

        public int Count { get; set; }

        public decimal Price { get; set; }

        public Guid OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; }
    }
}