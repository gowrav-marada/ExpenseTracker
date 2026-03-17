using ExpenseTracker.DTOs;

namespace ExpenseTracker.Services;

public interface ICategoryService
{
    Task<List<CategoryResponseDto>> GetAllAsync();
    Task<CategoryResponseDto?> GetByIdAsync(string id);
    Task<CategoryResponseDto> CreateAsync(CreateCategoryDto dto);
    Task<CategoryResponseDto?> UpdateAsync(string id, UpdateCategoryDto dto);
    Task<bool> DeleteAsync(string id);
}
