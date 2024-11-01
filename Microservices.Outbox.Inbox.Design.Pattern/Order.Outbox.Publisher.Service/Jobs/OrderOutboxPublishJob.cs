using MassTransit;
using Order.Outbox.Publisher.Service.Entities;
using Quartz;
using Shared.Events;
using System.Text.Json;

namespace Order.Outbox.Publisher.Service.Jobs
{
    public class OrderOutboxPublishJob : IJob
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public OrderOutboxPublishJob(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (OrderOutboxSingletonDatabase.DataReaderState)
            {
                OrderOutboxSingletonDatabase.DataReaderBusy();

                List<OrderOutbox> orderOutboxes = (await OrderOutboxSingletonDatabase
                    .QueryAsync<OrderOutbox>("Select * from OrderOutboxes where ProcessedDate is null order by OccuredOn asc"))
                    .ToList();

                foreach (var orderOutbox in orderOutboxes)
                {
                    if (orderOutbox.Type == nameof(OrderCreatedEvent))
                    {
                        OrderCreatedEvent orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(orderOutbox.Payload);
                        if (orderCreatedEvent != null )
                        {
                            await _publishEndpoint.Publish(orderCreatedEvent);
                            await OrderOutboxSingletonDatabase.ExecuteAsync($"Update OrderOutboxes set ProcessedDate = GetDate() where IdempotentToken = '{orderOutbox.IdempotentToken}'");
                        }
                    }
                }
                OrderOutboxSingletonDatabase.DataReaderReady();
                await Console.Out.WriteLineAsync("Order outbox table checked");
            }
        }
    }
}
