using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MyGarageSale.Models;

public class Item
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }
    
    [StringLength(500)]
    public string? ImagePath { get; set; } // Para compatibilidad hacia atrás
    
    public bool IsSold { get; set; } = false;
    
    // Control de stock - por defecto 1 (artículo único de garage sale)
    public int Stock { get; set; } = 1;
    
    // Interested count - increments each time someone requests this item
    public int InterestedCount { get; set; } = 0;
    
    // Badges configurables
    public bool IsNew { get; set; } = false;
    public bool IsHot { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Foreign key
    public int CategoryId { get; set; }
    
    // Market price (brand new)
    public decimal? MarketPrice { get; set; }
    // Show discount
    public bool ShowDiscount { get; set; } = false;
    
    // Navigation properties
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<ItemImage> Images { get; set; } = new List<ItemImage>();
    
    // Helper property to get the main image
    [NotMapped]
    public string? MainImagePath => Images.OrderBy(i => i.Order).FirstOrDefault()?.ImagePath ?? ImagePath;
    
    // Helper property to check availability
    [NotMapped]
    public bool IsAvailable => !IsSold && Stock > 0;
} 