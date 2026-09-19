using LinqRetrievalLab.Data;
using LinqRetrievalLab.DTOs;
using LinqRetrievalLab.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinqRetrievalLab.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    // ADD CATEGORY
    [HttpPost]
    public async Task<IActionResult> AddCategory(CategoryDto dto)
    {
        if (dto == null)
            return BadRequest("Category data is required.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Category name is required.");

        var category = new Category
        {
            Name = dto.Name
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return Ok(category);
    }

    // EDIT CATEGORY
    [HttpPut("{id}")]
    public async Task<IActionResult> EditCategory(
        int id,
        CategoryDto dto)
    {
        if (dto == null)
            return BadRequest("Category data is required.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Category name is required.");

        var category = await _context.Categories.FindAsync(id);

        if (category == null)
            return NotFound("Category not found.");

        category.Name = dto.Name;

        await _context.SaveChangesAsync();

        return Ok(category);
    }

    // DELETE CATEGORY
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return NotFound("Category not found.");

        if (category.Products.Any())
        {
            return BadRequest(
                "Cannot delete a category that has products."
            );
        }

        _context.Categories.Remove(category);

        await _context.SaveChangesAsync();

        return Ok("Category deleted successfully.");
    }
}
