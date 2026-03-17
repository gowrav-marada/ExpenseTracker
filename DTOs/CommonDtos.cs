namespace ExpenseTracker.DTOs;

public record ExpenseSummaryDto(
    decimal TotalAmount,
    int TotalCount,
    string Currency,
    DateTime FromDate,
    DateTime ToDate,
    List<CategoryBreakdownDto> ByCategory
);

public record CategoryBreakdownDto(
    string Category,
    decimal TotalAmount,
    int Count,
    double Percentage
);

public record PagedResultDto<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
