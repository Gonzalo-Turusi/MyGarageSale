using Microsoft.EntityFrameworkCore;
using MyGarageSale.Data;
using MyGarageSale.Models;

namespace MyGarageSale.Services;

public interface ICategoryService
{
    Task<List<Category>> GetAllCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(int id);
    Task<Category> CreateCategoryAsync(Category category);
    Task<Category> UpdateCategoryAsync(Category category);
    Task<bool> DeleteCategoryAsync(int id);
    Task<bool> CategoryHasItemsAsync(int id);
}

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(AppDbContext context, ILogger<CategoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        try
        {
            return await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all categories");
            return new List<Category>();
        }
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        try
        {
            return await _context.Categories
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category {CategoryId}", id);
            return null;
        }
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        try
        {
            category.CreatedAt = DateTime.UtcNow;
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Category created successfully with ID {CategoryId}", category.Id);
            return category;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            throw;
        }
    }

    public async Task<Category> UpdateCategoryAsync(Category category)
    {
        try
        {
            var existingCategory = await _context.Categories.FindAsync(category.Id);
            if (existingCategory == null)
            {
                throw new InvalidOperationException($"Category with ID {category.Id} not found");
            }

            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Category updated successfully with ID {CategoryId}", category.Id);
            return existingCategory;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", category.Id);
            throw;
        }
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        try
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return false;
            }

            // Check if category has items
            var hasItems = await CategoryHasItemsAsync(id);
            if (hasItems)
            {
                throw new InvalidOperationException("Cannot delete category that contains items");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Category deleted successfully with ID {CategoryId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", id);
            throw;
        }
    }

    public async Task<bool> CategoryHasItemsAsync(int id)
    {
        try
        {
            return await _context.Items.AnyAsync(i => i.CategoryId == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if category has items {CategoryId}", id);
            return true; // Return true to prevent deletion if there's an error
        }
    }
} 