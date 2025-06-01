using Microsoft.EntityFrameworkCore;
using MyGarageSale.Data;
using MyGarageSale.Models;

namespace MyGarageSale.Services;

public interface IItemService
{
    Task<List<Item>> GetAllItemsAsync();
    Task<List<Item>> GetAvailableItemsAsync();
    Task<List<Item>> GetItemsByCategoryAsync(int categoryId);
    Task<Item?> GetItemByIdAsync(int id);
    Task<Item> CreateItemAsync(Item item);
    Task<Item> UpdateItemAsync(Item item);
    Task<bool> DeleteItemAsync(int id);
    Task<bool> MarkAsSoldAsync(int id);
    Task<bool> MarkAsAvailableAsync(int id);
}

public class ItemService : IItemService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ItemService> _logger;

    public ItemService(AppDbContext context, ILogger<ItemService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Item>> GetAllItemsAsync()
    {
        try
        {
            return await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Images.OrderBy(img => img.Order))
                .OrderBy(i => i.IsSold)
                .ThenByDescending(i => i.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all items");
            return new List<Item>();
        }
    }

    public async Task<List<Item>> GetAvailableItemsAsync()
    {
        try
        {
            return await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Images.OrderBy(img => img.Order))
                .Where(i => !i.IsSold)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available items");
            return new List<Item>();
        }
    }

    public async Task<List<Item>> GetItemsByCategoryAsync(int categoryId)
    {
        try
        {
            return await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Images.OrderBy(img => img.Order))
                .Where(i => i.CategoryId == categoryId && !i.IsSold)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving items by category {CategoryId}", categoryId);
            return new List<Item>();
        }
    }

    public async Task<Item?> GetItemByIdAsync(int id)
    {
        try
        {
            return await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Images.OrderBy(img => img.Order))
                .FirstOrDefaultAsync(i => i.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving item {ItemId}", id);
            return null;
        }
    }

    public async Task<Item> CreateItemAsync(Item item)
    {
        try
        {
            item.CreatedAt = DateTime.UtcNow;
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            
            // Load the category for the created item
            await _context.Entry(item).Reference(i => i.Category).LoadAsync();
            await _context.Entry(item).Collection(i => i.Images).LoadAsync();
            
            _logger.LogInformation("Item created successfully with ID {ItemId}", item.Id);
            return item;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating item");
            throw;
        }
    }

    public async Task<Item> UpdateItemAsync(Item item)
    {
        try
        {
            var existingItem = await _context.Items.FindAsync(item.Id);
            if (existingItem == null)
            {
                throw new InvalidOperationException($"Item with ID {item.Id} not found");
            }

            existingItem.Title = item.Title;
            existingItem.Description = item.Description;
            existingItem.Price = item.Price;
            existingItem.CategoryId = item.CategoryId;
            existingItem.IsSold = item.IsSold;
            existingItem.UpdatedAt = DateTime.UtcNow;
            
            if (!string.IsNullOrEmpty(item.ImagePath))
            {
                existingItem.ImagePath = item.ImagePath;
            }

            await _context.SaveChangesAsync();
            
            // Load the category and images for the updated item
            await _context.Entry(existingItem).Reference(i => i.Category).LoadAsync();
            await _context.Entry(existingItem).Collection(i => i.Images).LoadAsync();
            
            _logger.LogInformation("Item updated successfully with ID {ItemId}", item.Id);
            return existingItem;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating item {ItemId}", item.Id);
            throw;
        }
    }

    public async Task<bool> DeleteItemAsync(int id)
    {
        try
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return false;
            }

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Item deleted successfully with ID {ItemId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting item {ItemId}", id);
            return false;
        }
    }

    public async Task<bool> MarkAsSoldAsync(int id)
    {
        try
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return false;
            }

            item.IsSold = true;
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Item marked as sold with ID {ItemId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking item as sold {ItemId}", id);
            return false;
        }
    }

    public async Task<bool> MarkAsAvailableAsync(int id)
    {
        try
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return false;
            }

            item.IsSold = false;
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Item marked as available with ID {ItemId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking item as available {ItemId}", id);
            return false;
        }
    }
} 