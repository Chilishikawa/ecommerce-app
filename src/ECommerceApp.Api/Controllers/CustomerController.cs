using ECommerceApp.Api.DTOs;
using ECommerceApp.Core.Entities;
using ECommerceApp.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _context;

    public CustomersController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/customers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetCustomers()
    {
        var customers = await _context.Customers
            .Select(c => new CustomerDTO
            {
                Id = c.Id,
                FullName = c.Name,
                Email = c.Email
            })
            .ToListAsync();
        return Ok(customers);
    }

    // GET: api/customers/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDTO>> GetCustomer(int id)
    {
        var c = await _context.Customers.FindAsync(id);
        if (c == null) return NotFound();

        return new CustomerDTO
        {
            Id = c.Id,
            FullName = c.Name,
            Email = c.Email
        };
    }

    // POST: api/customers
    [HttpPost]
    public async Task<ActionResult<CustomerDTO>> CreateCustomer(CreateCustomerDTO dto)
    {
        var customer = new Customer
        {
            Name = dto.Name,
            Email = dto.Email
        };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, new CustomerDTO
        {
            Id = customer.Id,
            FullName = customer.Name,
            Email = customer.Email
        });
    }

    // PUT: api/customers/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(int id, UpdateCustomerDTO dto)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound();

        if (dto.Name != null) customer.Name = dto.Name;
        if (dto.Email != null) customer.Email = dto.Email;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/customers/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound();

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
