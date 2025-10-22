using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.Api.DTOs;

public class UpdateCustomerDTO
{
    [StringLength(100)]
    public string? Name { get; set; }

    [EmailAddress]
    public string? Email { get; set; }
}
