using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Models.Contexts;
using Order.API.Models.DTOs;
using Order.API.Models.Entities;
using Order.API.Models.Enums;
using Shared;
using Shared.Events;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(configurator =>
{

    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration.GetConnectionString("RabbitMQ"));
    });
});

builder.Services.AddDbContext<OrderDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer"));
});

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();


app.MapPost("/create-order", async (CreateOrderDTO createOrder, OrderDbContext context, ISendEndpointProvider sendEndpointProvider) =>
{
    Order.API.Models.Entities.Order order = new()
    {
        BuyerId = createOrder.BuyerId,
        CreatedDate = DateTime.UtcNow,
        OrderStatus = OrderStatus.Suspend,
        TotalPrice = createOrder.OrderItems.Sum(oi => oi.Count * oi.Price),
        OrderItems = createOrder.OrderItems.Select(oi => new Order.API.Models.Entities.OrderItem
        {
            ProductId = oi.ProductId,
            Price = oi.Price,
            Count = oi.Count,
        }).ToList(),
    };

    await context.Orders.AddAsync(order);
    await context.SaveChangesAsync();

    var idempotentToken = Guid.NewGuid();
    OrderCreatedEvent orderCreatedEvent = new()
    {
        IdempotentToken = idempotentToken,
        OrderId = order.Id,
        BuyerId = createOrder.BuyerId,
        TotalPrice = createOrder.OrderItems.Sum(oi => oi.Price * oi.Count),
        OrderItems = createOrder.OrderItems.Select(item => new Shared.Messages.OrderItemMessage
        {
            ProductId = item.ProductId,
            Price = item.Price,
            Count = item.Count,
        }).ToList()
    };

    OrderOutbox orderOutbox = new()
    {
        IdempotentToken = idempotentToken,
        OccuredOn = DateTime.UtcNow,
        Payload = JsonSerializer.Serialize(orderCreatedEvent),
        Type = nameof(OrderCreatedEvent)
    };

    await context.OrderOutboxes.AddAsync(orderOutbox);
    await context.SaveChangesAsync();
});


app.Run();
