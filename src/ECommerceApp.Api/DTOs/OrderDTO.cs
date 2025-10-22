namespace ECommerceApp.Api.DTOs;

public class OrderDTO
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDTO> Items { get; set; } = new();
}
