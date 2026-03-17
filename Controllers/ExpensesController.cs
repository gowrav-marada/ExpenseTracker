using ExpenseTracker.DTOs;
using ExpenseTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _expenseService;

    public ExpensesController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<ExpenseResponseDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? categoryId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 10;

        var result = await _expenseService.GetAllAsync(page, pageSize, categoryId, from, to);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExpenseResponseDto>> GetById(string id)
    {
        var expense = await _expenseService.GetByIdAsync(id);
        if (expense is null) return NotFound(new { message = "Expense not found." });
        return Ok(expense);
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseResponseDto>> Create([FromBody] CreateExpenseDto dto)
    {
        var expense = await _expenseService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = expense.Id }, expense);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ExpenseResponseDto>> Update(string id, [FromBody] UpdateExpenseDto dto)
    {
        var expense = await _expenseService.UpdateAsync(id, dto);
        if (expense is null) return NotFound(new { message = "Expense not found." });
        return Ok(expense);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _expenseService.DeleteAsync(id);
        if (!deleted) return NotFound(new { message = "Expense not found." });
        return NoContent();
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ExpenseSummaryDto>> GetSummary(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string currency = "INR")
    {
        var fromDate = from ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var toDate = to ?? DateTime.UtcNow;

        var summary = await _expenseService.GetSummaryAsync(fromDate, toDate, currency);
        return Ok(summary);
    }
}
