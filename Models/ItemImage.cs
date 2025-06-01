using System.ComponentModel.DataAnnotations;

namespace MyGarageSale.Models;

public class ItemImage
{
    public int Id { get; set; }
    
    [Required]
    public string ImagePath { get; set; } = string.Empty;
    
    public string? AltText { get; set; }
    
    public int Order { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign key
    public int ItemId { get; set; }
    public Item Item { get; set; } = null!;
} 