using ECommerceApp.Api.DTOs;
using ECommerceApp.Core.Entities;
using ECommerceApp.Infrastructure.Data;
using ECommerceApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Api.Controllers;

//[Authorize]
[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly PaymentService _paymentService;

    public OrdersController(AppDbContext context, PaymentService paymentService)
    {
        _context = context;
        _paymentService = paymentService;
    }

    // GET: api/orders
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrders()
    {
        var orders = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
            .Select(o => new OrderDTO
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer.Name,
                //CreatedAt = o.CreatedAt,
                Items = o.Items.Select(oi => new OrderItemDTO
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    Price = oi.Product.Price
                }).ToList()
            }).ToListAsync();

        return Ok(orders);
    }

    // POST: api/orders
    [HttpPost]
    public async Task<ActionResult<OrderDTO>> CreateOrder(CreateOrderDTO dto)
    {
        var order = new Order
        {
            CustomerId = dto.CustomerId,
            //CreatedAt = DateTime.UtcNow,
            Items = dto.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }).ToList()
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Devuelve el OrderDTO completo con Include
        var savedOrder = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == order.Id);

        if (savedOrder == null) return BadRequest();

        return new OrderDTO
        {
            Id = savedOrder.Id,
            CustomerId = savedOrder.CustomerId,
            CustomerName = savedOrder.Customer.Name,
            //CreatedAt = savedOrder.CreatedAt,
            Items = savedOrder.Items.Select(oi => new OrderItemDTO
            {
                ProductId = oi.ProductId,
                ProductName = oi.Product.Name,
                Quantity = oi.Quantity,
                Price = oi.Product.Price
            }).ToList()
        };
    }

    [HttpGet("validate-payment/{orderId}")]
    public async Task<IActionResult> ValidatePayment(string orderId)
    {
        var ok = await _paymentService.ValidatePaymentAsync(orderId);
        return ok ? Ok("Pago válido ✅") : StatusCode(500, "Error validando pago ❌");
    }
}
