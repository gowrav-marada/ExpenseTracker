using ExpenseTracker.DTOs;

namespace ExpenseTracker.Services;

public interface IExpenseService
{
    Task<PagedResultDto<ExpenseResponseDto>> GetAllAsync(int page, int pageSize, string? categoryId, DateTime? from, DateTime? to);
    Task<ExpenseResponseDto?> GetByIdAsync(string id);
    Task<ExpenseResponseDto> CreateAsync(CreateExpenseDto dto);
    Task<ExpenseResponseDto?> UpdateAsync(string id, UpdateExpenseDto dto);
    Task<bool> DeleteAsync(string id);
    Task<ExpenseSummaryDto> GetSummaryAsync(DateTime from, DateTime to, string currency);
}
