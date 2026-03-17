using ExpenseTracker.DTOs;
using ExpenseTracker.Mappings;
using ExpenseTracker.Models;
using ExpenseTracker.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ExpenseTracker.Services;

public class CategoryService : ICategoryService
{
    private readonly IMongoCollection<Category> _categories;
    private readonly IUserContext _userContext;

    public CategoryService(IOptions<MongoDbSettings> settings, IUserContext userContext)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _categories = database.GetCollection<Category>("categories");
        _userContext = userContext;
    }

    public async Task<List<CategoryResponseDto>> GetAllAsync()
    {
        var categories = await _categories.Find(c => c.UserId == _userContext.UserId)
            .SortBy(c => c.Name)
            .ToListAsync();

        return categories.Select(c => c.ToDto()).ToList();
    }

    public async Task<CategoryResponseDto?> GetByIdAsync(string id)
    {
        var category = await _categories.Find(c => c.Id == id && c.UserId == _userContext.UserId).FirstOrDefaultAsync();
        return category?.ToDto();
    }

    public async Task<CategoryResponseDto> CreateAsync(CreateCategoryDto dto)
    {
        var category = dto.ToEntity(_userContext.UserId);
        await _categories.InsertOneAsync(category);
        return category.ToDto();
    }

    public async Task<CategoryResponseDto?> UpdateAsync(string id, UpdateCategoryDto dto)
    {
        var category = await _categories.Find(c => c.Id == id && c.UserId == _userContext.UserId).FirstOrDefaultAsync();
        if (category is null) return null;

        category.ApplyUpdate(dto);
        await _categories.ReplaceOneAsync(c => c.Id == id, category);
        return category.ToDto();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _categories.DeleteOneAsync(c => c.Id == id && c.UserId == _userContext.UserId);
        return result.DeletedCount > 0;
    }
}
