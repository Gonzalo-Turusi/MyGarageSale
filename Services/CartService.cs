using MyGarageSale.Models;

namespace MyGarageSale.Services;

public interface ICartService
{
    event Action? OnCartUpdated;
    List<CartItem> GetCartItems();
    void AddToCart(Item item);
    void RemoveFromCart(int itemId);
    void UpdateQuantity(int itemId, int quantity);
    void ClearCart();
    int GetCartCount();
    decimal GetCartTotal();
    bool IsItemInCart(int itemId);
}

public class CartService : ICartService
{
    private readonly List<CartItem> _cartItems = new();
    private readonly ILogger<CartService> _logger;

    public event Action? OnCartUpdated;

    public CartService(ILogger<CartService> logger)
    {
        _logger = logger;
    }

    public List<CartItem> GetCartItems()
    {
        return _cartItems.ToList();
    }

    public bool IsItemInCart(int itemId)
    {
        return _cartItems.Any(ci => ci.Item.Id == itemId);
    }

    public void AddToCart(Item item)
    {
        try
        {
            if (item.IsSold || !item.IsAvailable)
            {
                _logger.LogWarning("Attempted to add unavailable item {ItemId} to cart", item.Id);
                return;
            }

            var existingItem = _cartItems.FirstOrDefault(ci => ci.Item.Id == item.Id);
            if (existingItem != null)
            {
                _logger.LogInformation("Item {ItemId} already in cart, quantity remains 1", item.Id);
                return; // No permitir duplicados ya que solo hay 1 de cada artículo
            }
            
            _cartItems.Add(new CartItem
            {
                Item = item,
                Quantity = 1 // Siempre 1 para garage sale
            });
            
            _logger.LogInformation("Added item {ItemId} to cart", item.Id);
            OnCartUpdated?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item {ItemId} to cart", item.Id);
        }
    }

    public void RemoveFromCart(int itemId)
    {
        try
        {
            var item = _cartItems.FirstOrDefault(ci => ci.Item.Id == itemId);
            if (item != null)
            {
                _cartItems.Remove(item);
                _logger.LogInformation("Removed item {ItemId} from cart", itemId);
                OnCartUpdated?.Invoke();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing item {ItemId} from cart", itemId);
        }
    }

    public void UpdateQuantity(int itemId, int quantity)
    {
        try
        {
            var item = _cartItems.FirstOrDefault(ci => ci.Item.Id == itemId);
            if (item != null)
            {
                if (quantity <= 0)
                {
                    RemoveFromCart(itemId);
                }
                else
                {
                    // Para garage sale, máximo 1 de cada artículo
                    item.Quantity = 1;
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

    public void ClearCart()
    {
        try
        {
            _cartItems.Clear();
            _logger.LogInformation("Cart cleared");
            OnCartUpdated?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cart");
        }
    }

    public int GetCartCount()
    {
        return _cartItems.Sum(ci => ci.Quantity);
    }

    public decimal GetCartTotal()
    {
        return _cartItems.Sum(ci => ci.Item.Price * ci.Quantity);
    }
}

// Model for cart items
public class CartItem
{
    public required Item Item { get; set; }
    public int Quantity { get; set; } = 1;
}