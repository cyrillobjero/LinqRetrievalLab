using LinqRetrievalLab.Data;
using LinqRetrievalLab.DTO;
using LinqRetrievalLab.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinqRetrievalLab.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    // ADD PRODUCT
    [HttpPost]
    public async Task<IActionResult> AddProduct(ProductDto dto)
    {
        if (dto == null)
            return BadRequest("Product data is required.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Product name is required.");

        if (dto.Price < 0)
            return BadRequest("Price cannot be negative.");

        if (dto.Stock < 0)
            return BadRequest("Stock cannot be negative.");

        var category = await _context.Categories.FindAsync(dto.CategoryId);

        if (category == null)
            return NotFound("Category not found.");

        var product = new Product
        {
            Name = dto.Name,
            Price = dto.Price,
            Stock = dto.Stock,
            CategoryId = dto.CategoryId
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetProductWithCategory),
            new { id = product.Id },
            product
        );
    }

    // EDIT PRODUCT
    [HttpPut("{id}")]
    public async Task<IActionResult> EditProduct(int id, ProductDto dto)
    {
        if (dto == null)
            return BadRequest("Product data is required.");

        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound("Product not found.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Product name is required.");

        if (dto.Price < 0)
            return BadRequest("Price cannot be negative.");

        if (dto.Stock < 0)
            return BadRequest("Stock cannot be negative.");

        var category = await _context.Categories.FindAsync(dto.CategoryId);

        if (category == null)
            return NotFound("Category not found.");

        product.Name = dto.Name;
        product.Price = dto.Price;
        product.Stock = dto.Stock;
        product.CategoryId = dto.CategoryId;

        await _context.SaveChangesAsync();

        return Ok(product);
    }

    // DELETE PRODUCT
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound("Product not found.");

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return Ok("Product deleted successfully.");
    }

    // GET PRODUCT WITH CATEGORY
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductWithCategory(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound("Product not found.");

        return Ok(product);
    }

    // GET PRODUCTS BY CATEGORY
    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetProductsByCategory(int categoryId)
    {
        var category = await _context.Categories.FindAsync(categoryId);

        if (category == null)
            return NotFound("Category not found.");

        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();

        return Ok(products);
    }

    // SEARCH PRODUCTS BY NAME
    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Search name is required.");

        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.Name.Contains(name))
            .ToListAsync();

        return Ok(products);
    }

    // GET PRODUCTS BY PRICE RANGE
    [HttpGet("price-range")]
    public async Task<IActionResult> GetProductsByPriceRange(
        decimal lowerLimit,
        decimal upperLimit)
    {
        if (lowerLimit < 0 || upperLimit < 0)
            return BadRequest("Price cannot be negative.");

        if (lowerLimit > upperLimit)
            return BadRequest(
                "Lower limit cannot be greater than upper limit."
            );

        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.Price >= lowerLimit &&
                        p.Price <= upperLimit)
            .ToListAsync();

        return Ok(products);
    }

    // TOTAL STOCK
    [HttpGet("statistics/total-stock")]
    public async Task<IActionResult> GetTotalStock()
    {
        var totalStock = await _context.Products
            .SumAsync(p => p.Stock);

        return Ok(new
        {
            TotalStock = totalStock
        });
    }

    // AVERAGE PRICE
    [HttpGet("statistics/average-price")]
    public async Task<IActionResult> GetAveragePrice()
    {
        var count = await _context.Products.CountAsync();

        if (count == 0)
        {
            return Ok(new
            {
                AveragePrice = 0
            });
        }

        var averagePrice = await _context.Products
            .AverageAsync(p => p.Price);

        return Ok(new
        {
            AveragePrice = averagePrice
        });
    }

    // TOTAL PRODUCT COUNT
    [HttpGet("statistics/count")]
    public async Task<IActionResult> GetProductCount()
    {
        var count = await _context.Products.CountAsync();

        return Ok(new
        {
            TotalProducts = count
        });
    }
}