using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.DTOs;

public record CreateExpenseDto(
    [Required, StringLength(200)] string Title,
    [Required, Range(0.01, double.MaxValue)] decimal Amount,
    [StringLength(3)] string Currency,
    [Required, StringLength(100)] string CategoryId,
    string? Note,
    DateTime? Date
);

public record UpdateExpenseDto(
    [StringLength(200)] string? Title,
    [Range(0.01, double.MaxValue)] decimal? Amount,
    [StringLength(3)] string? Currency,
    [StringLength(100)] string? CategoryId,
    string? Note,
    DateTime? Date
);

public record ExpenseResponseDto(
    string Id,
    string Title,
    decimal Amount,
    string Currency,
    string CategoryId,
    string? Note,
    DateTime Date,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
