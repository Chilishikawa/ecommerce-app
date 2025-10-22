namespace ECommerceApp.Api.DTOs;

public class CreateOrderDTO
{
    public int CustomerId { get; set; }
    public List<OrderItemCreateDTO> Items { get; set; } = new();
}

public class OrderItemCreateDTO
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
