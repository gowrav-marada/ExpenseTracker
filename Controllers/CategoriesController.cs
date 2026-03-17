using ExpenseTracker.DTOs;
using ExpenseTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryResponseDto>>> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryResponseDto>> GetById(string id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category is null) return NotFound(new { message = "Category not found." });
        return Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponseDto>> Create([FromBody] CreateCategoryDto dto)
    {
        var category = await _categoryService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryResponseDto>> Update(string id, [FromBody] UpdateCategoryDto dto)
    {
        var category = await _categoryService.UpdateAsync(id, dto);
        if (category is null) return NotFound(new { message = "Category not found." });
        return Ok(category);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _categoryService.DeleteAsync(id);
        if (!deleted) return NotFound(new { message = "Category not found." });
        return NoContent();
    }
}
