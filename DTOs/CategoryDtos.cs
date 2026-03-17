using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.DTOs;

public record CreateCategoryDto(
    [Required, StringLength(100)] string Name,
    [StringLength(10)] string? Icon,
    [StringLength(7)] string? Color
);

public record UpdateCategoryDto(
    [StringLength(100)] string? Name,
    [StringLength(10)] string? Icon,
    [StringLength(7)] string? Color
);

public record CategoryResponseDto(
    string Id,
    string Name,
    string Icon,
    string Color,
    DateTime CreatedAt
);
