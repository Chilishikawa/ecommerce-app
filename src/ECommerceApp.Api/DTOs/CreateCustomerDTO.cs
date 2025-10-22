using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.Api.DTOs;

public class CreateCustomerDTO
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
