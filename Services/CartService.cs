using MyGarageSale.Models;
using Microsoft.JSInterop;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyGarageSale.Services;

public interface ICartService
{
    event Action? OnCartUpdated;
    Task<List<CartItem>> GetCartItemsAsync();
    Task AddToCartAsync(Item item);
    Task RemoveFromCartAsync(int itemId);
    Task UpdateQuantityAsync(int itemId, int quantity);
    Task ClearCartAsync();
    Task<int> GetCartCountAsync();
    Task<decimal> GetCartTotalAsync();
    Task<bool> IsItemInCartAsync(int itemId);
    void InitializeAfterRender();
}

public class CartService : ICartService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<CartService> _logger;
    private const string CART_KEY = "my-garage-sale-cart";
    private bool _isInitialized = false;
    private List<CartItemDto> _fallbackCartItems = new();

    public event Action? OnCartUpdated;

    public CartService(IJSRuntime jsRuntime, ILogger<CartService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public void InitializeAfterRender()
    {
        _isInitialized = true;
    }

    private bool CanUseJavaScript()
    {
        return _isInitialized && _jsRuntime is IJSInProcessRuntime || _jsRuntime.GetType().Name != "UnsupportedJavaScriptRuntime";
    }

    public async Task<List<CartItem>> GetCartItemsAsync()
    {
        try
        {
            if (!CanUseJavaScript())
            {
                // Durante prerendering, devolver lista vacía
                return ConvertDtosToCartItems(_fallbackCartItems);
            }

            var cartJson = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", CART_KEY);
            if (string.IsNullOrEmpty(cartJson))
                return new List<CartItem>();

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var cartItemDtos = JsonSerializer.Deserialize<List<CartItemDto>>(cartJson, options);
            if (cartItemDtos == null) return new List<CartItem>();

            return ConvertDtosToCartItems(cartItemDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cart items from session storage");
            return new List<CartItem>();
        }
    }

    private List<CartItem> ConvertDtosToCartItems(List<CartItemDto> cartItemDtos)
    {
        return cartItemDtos.Select(dto => new CartItem
        {
            Item = new Item
            {
                Id = dto.ItemId,
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                ImagePath = dto.ImagePath,
                IsSold = dto.IsSold,
                Stock = dto.Stock,
                CategoryId = dto.CategoryId,
                IsNew = dto.IsNew,
                IsHot = dto.IsHot,
                CreatedAt = DateTime.UtcNow,
                Category = dto.CategoryId > 0 ? new Category 
                { 
                    Id = dto.CategoryId, 
                    Name = dto.CategoryName ?? "",
                    CreatedAt = DateTime.UtcNow
                } : null!,
                // Crear una imagen principal si existe
                Images = !string.IsNullOrEmpty(dto.MainImagePath) ? new List<ItemImage>
                {
                    new ItemImage
                    {
                        Id = 0,
                        ItemId = dto.ItemId,
                        ImagePath = dto.MainImagePath,
                        Order = 0,
                        CreatedAt = DateTime.UtcNow
                    }
                } : new List<ItemImage>()
            },
            Quantity = dto.Quantity
        }).ToList();
    }

    private async Task SaveCartItemsAsync(List<CartItem> cartItems)
    {
        try
        {
            // Convertir a DTOs para serialización
            var cartItemDtos = cartItems.Select(ci => new CartItemDto
            {
                ItemId = ci.Item.Id,
                Title = ci.Item.Title,
                Description = ci.Item.Description,
                Price = ci.Item.Price,
                ImagePath = ci.Item.ImagePath,
                MainImagePath = GetMainImagePath(ci.Item),
                IsSold = ci.Item.IsSold,
                Stock = ci.Item.Stock,
                CategoryId = ci.Item.CategoryId,
                CategoryName = ci.Item.Category?.Name,
                IsNew = ci.Item.IsNew,
                IsHot = ci.Item.IsHot,
                Quantity = ci.Quantity
            }).ToList();

            if (!CanUseJavaScript())
            {
                // Durante prerendering, guardar en fallback
                _fallbackCartItems = cartItemDtos;
                return;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var cartJson = JsonSerializer.Serialize(cartItemDtos, options);
            await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", CART_KEY, cartJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving cart items to session storage");
        }
    }

    private string? GetMainImagePath(Item item)
    {
        // Prioridad: 1) Primera imagen de la colección, 2) ImagePath legacy
        var firstImage = item.Images?.OrderBy(i => i.Order).FirstOrDefault();
        if (firstImage != null && !string.IsNullOrEmpty(firstImage.ImagePath))
            return firstImage.ImagePath;
            
        return item.ImagePath;
    }

    public async Task<bool> IsItemInCartAsync(int itemId)
    {
        var cartItems = await GetCartItemsAsync();
        return cartItems.Any(ci => ci.Item.Id == itemId);
    }

    public async Task AddToCartAsync(Item item)
    {
        try
        {
            if (item.IsSold || !item.IsAvailable)
            {
                _logger.LogWarning("Attempted to add unavailable item {ItemId} to cart", item.Id);
                return;
            }

            var cartItems = await GetCartItemsAsync();
            var existingItem = cartItems.FirstOrDefault(ci => ci.Item.Id == item.Id);
            
            if (existingItem != null)
            {
                _logger.LogInformation("Item {ItemId} already in cart, quantity remains 1", item.Id);
                return; // No permitir duplicados ya que solo hay 1 de cada artículo
            }
            
            cartItems.Add(new CartItem
            {
                Item = item,
                Quantity = 1 // Siempre 1 para garage sale
            });
            
            await SaveCartItemsAsync(cartItems);
            
            _logger.LogInformation("Added item {ItemId} to cart", item.Id);
            OnCartUpdated?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item {ItemId} to cart", item.Id);
        }
    }

    public async Task RemoveFromCartAsync(int itemId)
    {
        try
        {
            var cartItems = await GetCartItemsAsync();
            var item = cartItems.FirstOrDefault(ci => ci.Item.Id == itemId);
            
            if (item != null)
            {
                cartItems.Remove(item);
                await SaveCartItemsAsync(cartItems);
                _logger.LogInformation("Removed item {ItemId} from cart", itemId);
                OnCartUpdated?.Invoke();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing item {ItemId} from cart", itemId);
        }
    }

    public async Task UpdateQuantityAsync(int itemId, int quantity)
    {
        try
        {
            var cartItems = await GetCartItemsAsync();
            var item = cartItems.FirstOrDefault(ci => ci.Item.Id == itemId);
            
            if (item != null)
            {
                if (quantity <= 0)
                {
                    await RemoveFromCartAsync(itemId);
                }
                else
                {
                    // Para garage sale, máximo 1 de cada artículo
                    item.Quantity = 1;
                    await SaveCartItemsAsync(cartItems);
                    _logger.LogInformation("Quantity for item {ItemId} set to 1 (garage sale limit)", itemId);
                    OnCartUpdated?.Invoke();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating quantity for item {ItemId}", itemId);
        }
    }

    public async Task ClearCartAsync()
    {
        try
        {
            if (!CanUseJavaScript())
            {
                _fallbackCartItems.Clear();
                return;
            }

            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", CART_KEY);
            _logger.LogInformation("Cart cleared");
            OnCartUpdated?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cart");
        }
    }

    public async Task<int> GetCartCountAsync()
    {
        var cartItems = await GetCartItemsAsync();
        return cartItems.Sum(ci => ci.Quantity);
    }

    public async Task<decimal> GetCartTotalAsync()
    {
        var cartItems = await GetCartItemsAsync();
        return cartItems.Sum(ci => ci.Item.Price * ci.Quantity);
    }
}

// DTO para serialización JSON - sin referencias circulares ni propiedades calculadas
public class CartItemDto
{
    public int ItemId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImagePath { get; set; }
    public string? MainImagePath { get; set; }
    public bool IsSold { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public bool IsNew { get; set; }
    public bool IsHot { get; set; }
    public int Quantity { get; set; } = 1;
}

// Model for cart items (mantiene la estructura original)
public class CartItem
{
    public required Item Item { get; set; }
    public int Quantity { get; set; } = 1;
}