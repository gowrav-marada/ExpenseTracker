using ExpenseTracker.DTOs;
using ExpenseTracker.Models;

namespace ExpenseTracker.Mappings;

public static class ExpenseMappings
{
    public static Expense ToEntity(this CreateExpenseDto dto, string userId) => new()
    {
        UserId = userId,
        Title = dto.Title,
        Amount = dto.Amount,
        Currency = dto.Currency ?? "INR",
        CategoryId = dto.CategoryId,
        Note = dto.Note,
        Date = dto.Date ?? DateTime.UtcNow,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    public static ExpenseResponseDto ToDto(this Expense entity) => new(
        entity.Id,
        entity.Title,
        entity.Amount,
        entity.Currency,
        entity.CategoryId,
        entity.Note,
        entity.Date,
        entity.CreatedAt,
        entity.UpdatedAt
    );

    public static void ApplyUpdate(this Expense entity, UpdateExpenseDto dto)
    {
        if (dto.Title is not null) entity.Title = dto.Title;
        if (dto.Amount.HasValue) entity.Amount = dto.Amount.Value;
        if (dto.Currency is not null) entity.Currency = dto.Currency;
        if (dto.CategoryId is not null) entity.CategoryId = dto.CategoryId;
        if (dto.Note is not null) entity.Note = dto.Note;
        if (dto.Date.HasValue) entity.Date = dto.Date.Value;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}

public static class CategoryMappings
{
    public static Category ToEntity(this CreateCategoryDto dto, string userId) => new()
    {
        UserId = userId,
        Name = dto.Name,
        Icon = dto.Icon ?? "??",
        Color = dto.Color ?? "#6366f1",
        CreatedAt = DateTime.UtcNow
    };

    public static CategoryResponseDto ToDto(this Category entity) => new(
        entity.Id,
        entity.Name,
        entity.Icon,
        entity.Color,
        entity.CreatedAt
    );

    public static void ApplyUpdate(this Category entity, UpdateCategoryDto dto)
    {
        if (dto.Name is not null) entity.Name = dto.Name;
        if (dto.Icon is not null) entity.Icon = dto.Icon;
        if (dto.Color is not null) entity.Color = dto.Color;
    }
}
