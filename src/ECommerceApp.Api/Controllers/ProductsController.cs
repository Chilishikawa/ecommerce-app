using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceApp.Infrastructure.Data;
using ECommerceApp.Core.Entities;
using ECommerceApp.Api.DTOs;
using ECommerceApp.Api.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace ECommerceApp.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly CacheService _cacheService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(AppDbContext context, CacheService cacheService, ILogger<ProductsController> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
    }

    // GET api/products
    //[Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts()
    {
        string cacheKey = "products_all";
        var cachedProducts = await _cacheService.GetAsync<List<ProductDTO>>(cacheKey);
        
        if (cachedProducts is not null)
        {
            _logger.LogInformation("Sirviendo desde caché Redis");
            return Ok(cachedProducts);
        }

        var products = await _context.Products
            .Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                CategoryId = p.CategoryId
            })
            .ToListAsync();

        await _cacheService.SetAsync(cacheKey, products, seconds: 120);

        _logger.LogInformation("Datos guardados en caché Redis");
        return Ok(products);
    }

    // GET api/products/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDTO>> GetProduct(int id)
    {
        var product = await _context.Products
            .Where(p => p.Id == id)
            .Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                CategoryId = p.CategoryId
            })
            .FirstOrDefaultAsync();

        if (product == null) return NotFound();
        return Ok(product);
    }

    // POST api/products
    [HttpPost]
    public async Task<ActionResult<ProductDTO>> CreateProduct(CreateProductDTO dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            CategoryId = dto.CategoryId
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Invalida caché
        await _cacheService.RemoveAsync("products_all");

        var result = new ProductDTO
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            CategoryId = product.CategoryId
        };

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, result);
    }

    // PUT api/products/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, CreateProductDTO dto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.Stock = dto.Stock;
        product.CategoryId = dto.CategoryId;

        await _context.SaveChangesAsync();

        // Invalida caché
        await _cacheService.RemoveAsync("products_all");

        return NoContent();
    }

    // DELETE api/products/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        // Invalida caché
        await _cacheService.RemoveAsync("products_all");

        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchProduct(int id, UpdateProductDTO dto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        if (dto.Name != null) product.Name = dto.Name;
        if (dto.Description != null) product.Description = dto.Description;
        if (dto.Price.HasValue) product.Price = dto.Price.Value;
        if (dto.Stock.HasValue) product.Stock = dto.Stock.Value;
        if (dto.CategoryId.HasValue) product.CategoryId = dto.CategoryId.Value;

        await _context.SaveChangesAsync();
        await _cacheService.RemoveAsync("products_all");
        return NoContent();
    }

    [HttpGet("debug/throw")]
    public IActionResult Throw() => throw new Exception("Prueba de excepción");
}

