using Shared.Messages;

namespace Shared.Events
{
    public class OrderCreatedEvent
    {
        public long OrderId { get; set; }
        public long BuyerId { get; set; }
        public decimal TotalPrice { get; set; }
        public List<OrderItemMessage> OrderItems { get; set; }
    }
}
