using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Mvc;
using Order.API.Models;
using Order.API.ViewModels;
using Shared.Events;

namespace Order.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderApiDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrdersController(OrderApiDbContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrder createOrder)
        {
            Models.Entities.Order order = new Models.Entities.Order
            {
                OrderId = Guid.NewGuid(),
                BuyerId = createOrder.BuyerId,
                CreatedDate = DateTime.UtcNow,
                OrderStatus = Models.Enums.OrderStatus.Suspended,

            };

            order.OrderItems = createOrder.OrderItems.Select(oi => new Models.Entities.OrderItem
            {
                Count = oi.Count,
                Price = oi.Price,
                ProductId = oi.ProductId,
            }).ToList();

            order.TotalPrice = order.OrderItems.Sum(oi => oi.Price * oi.Count);

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            await _publishEndpoint.Publish(new OrderCreatedEvent
            {
                OrderId = order.OrderId,
                BuyerId = order.BuyerId,
                TotalPrice = order.TotalPrice,
                OrderItems = order.OrderItems.Select(oi => new Shared.Messages.OrderItemMessage
                {
                    ProductId = oi.ProductId,
                    Count = oi.Count
                }).ToList()
            });

            return Ok();
        }
    }
}