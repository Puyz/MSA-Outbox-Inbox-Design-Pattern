using Order.API.Models.Enums;

namespace Order.API.Models.Entities
{
    public class Order
    {
        public long Id { get; set; }
        public long BuyerId { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
