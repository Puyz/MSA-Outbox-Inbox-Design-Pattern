using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using Stock.Service.Models.Contexts;
using Stock.Service.Models.Entities;
using System.Text.Json;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly StockDbContext _stockDbContext;

        public OrderCreatedEventConsumer(StockDbContext stockDbContext)
        {
            _stockDbContext = stockDbContext;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            await _stockDbContext.OrderInboxes.AddAsync(new()
            {
                Processed = false,
                Payload = JsonSerializer.Serialize(context.Message)
            });
            await _stockDbContext.SaveChangesAsync();


            // Inbox için de bir worker Service (Quartz olabilir) oluşturmalıyız.
            List<OrderInbox> orderInboxes = await _stockDbContext.OrderInboxes.Where(i => i.Processed == false).ToListAsync();
            foreach (var order in orderInboxes)
            {
                OrderCreatedEvent orderCreatedEvent =  JsonSerializer.Deserialize<OrderCreatedEvent>(order.Payload);
                await Console.Out.WriteLineAsync($"{orderCreatedEvent.OrderId} Order ID değerine karşılık olan siparişin stok işlemleri tamamlandı.");
                order.Processed = true;
                await _stockDbContext.SaveChangesAsync();
            }
        }
    }
}
