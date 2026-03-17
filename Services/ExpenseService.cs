using ExpenseTracker.DTOs;
using ExpenseTracker.Mappings;
using ExpenseTracker.Models;
using ExpenseTracker.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ExpenseTracker.Services;

public class ExpenseService : IExpenseService
{
    private readonly IMongoCollection<Expense> _expenses;
    private readonly IMongoCollection<Category> _categories;
    private readonly IUserContext _userContext;

    public ExpenseService(IOptions<MongoDbSettings> settings, IUserContext userContext)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _expenses = database.GetCollection<Expense>("expenses");
        _categories = database.GetCollection<Category>("categories");
        _userContext = userContext;
    }

    public async Task<PagedResultDto<ExpenseResponseDto>> GetAllAsync(
        int page, int pageSize, string? categoryId, DateTime? from, DateTime? to)
    {
        var filterBuilder = Builders<Expense>.Filter;
        var filter = filterBuilder.Eq(e => e.UserId, _userContext.UserId);

        if (!string.IsNullOrWhiteSpace(categoryId))
            filter &= filterBuilder.Eq(e => e.CategoryId, categoryId);

        if (from.HasValue)
            filter &= filterBuilder.Gte(e => e.Date, from.Value);

        if (to.HasValue)
            filter &= filterBuilder.Lte(e => e.Date, to.Value);

        var totalCount = await _expenses.CountDocumentsAsync(filter);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var expenses = await _expenses
            .Find(filter)
            .SortByDescending(e => e.Date)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return new PagedResultDto<ExpenseResponseDto>(
            expenses.Select(e => e.ToDto()).ToList(),
            (int)totalCount,
            page,
            pageSize,
            totalPages
        );
    }

    public async Task<ExpenseResponseDto?> GetByIdAsync(string id)
    {
        var expense = await _expenses.Find(e => e.Id == id && e.UserId == _userContext.UserId).FirstOrDefaultAsync();
        return expense?.ToDto();
    }

    public async Task<ExpenseResponseDto> CreateAsync(CreateExpenseDto dto)
    {
        var expense = dto.ToEntity(_userContext.UserId);
        await _expenses.InsertOneAsync(expense);
        return expense.ToDto();
    }

    public async Task<ExpenseResponseDto?> UpdateAsync(string id, UpdateExpenseDto dto)
    {
        var expense = await _expenses.Find(e => e.Id == id && e.UserId == _userContext.UserId).FirstOrDefaultAsync();
        if (expense is null) return null;

        expense.ApplyUpdate(dto);
        await _expenses.ReplaceOneAsync(e => e.Id == id, expense);
        return expense.ToDto();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _expenses.DeleteOneAsync(e => e.Id == id && e.UserId == _userContext.UserId);
        return result.DeletedCount > 0;
    }

    public async Task<ExpenseSummaryDto> GetSummaryAsync(DateTime from, DateTime to, string currency)
    {
        var filter = Builders<Expense>.Filter.Eq(e => e.UserId, _userContext.UserId)
                   & Builders<Expense>.Filter.Gte(e => e.Date, from)
                   & Builders<Expense>.Filter.Lte(e => e.Date, to)
                   & Builders<Expense>.Filter.Eq(e => e.Currency, currency);

        var expenses = await _expenses.Find(filter).ToListAsync();

        var categoryIds = expenses.Select(e => e.CategoryId).Distinct().ToList();
        var categories = await _categories
            .Find(Builders<Category>.Filter.In(c => c.Id, categoryIds))
            .ToListAsync();
        var categoryNameMap = categories.ToDictionary(c => c.Id, c => c.Name);

        var totalAmount = expenses.Sum(e => e.Amount);
        var totalCount = expenses.Count;

        var byCategory = expenses
            .GroupBy(e => e.CategoryId)
            .Select(g => new CategoryBreakdownDto(
                categoryNameMap.GetValueOrDefault(g.Key, "Unknown"),
                g.Sum(e => e.Amount),
                g.Count(),
                totalAmount > 0 ? Math.Round((double)(g.Sum(e => e.Amount) / totalAmount) * 100, 2) : 0
            ))
            .OrderByDescending(c => c.TotalAmount)
            .ToList();

        return new ExpenseSummaryDto(totalAmount, totalCount, currency, from, to, byCategory);
    }
}
