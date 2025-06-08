using System.ComponentModel.DataAnnotations;

namespace MyGarageSale.Models;

public class PurchaseRequest
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string CustomerEmail { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string CustomerPhone { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Comments { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsProcessed { get; set; } = false;
    
    // Navigation property for items in the cart
    public virtual ICollection<PurchaseRequestItem> Items { get; set; } = new List<PurchaseRequestItem>();
}

public class PurchaseRequestItem
{
    public int Id { get; set; }
    
    public int PurchaseRequestId { get; set; }
    
    public int ItemId { get; set; }
    
    public int Quantity { get; set; } = 1;
    
    // Navigation properties
    public virtual PurchaseRequest PurchaseRequest { get; set; } = null!;
    public virtual Item Item { get; set; } = null!;
} 