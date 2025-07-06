using Microsoft.EntityFrameworkCore;
using MyGarageSale.Models;

namespace MyGarageSale.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<Category> Categories { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<ItemImage> ItemImages { get; set; }
    public DbSet<PurchaseRequest> PurchaseRequests { get; set; }
    public DbSet<PurchaseRequestItem> PurchaseRequestItems { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure relationships
        modelBuilder.Entity<Item>()
            .HasOne(i => i.Category)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
            
        modelBuilder.Entity<ItemImage>()
            .HasOne(ii => ii.Item)
            .WithMany(i => i.Images)
            .HasForeignKey(ii => ii.ItemId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<PurchaseRequestItem>()
            .HasOne(pri => pri.PurchaseRequest)
            .WithMany(pr => pr.Items)
            .HasForeignKey(pri => pri.PurchaseRequestId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<PurchaseRequestItem>()
            .HasOne(pri => pri.Item)
            .WithMany()
            .HasForeignKey(pri => pri.ItemId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Seed data
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Electrónicos", Description = "Dispositivos electrónicos y gadgets", CreatedAt = new DateTime(2024, 1, 1) },
            new Category { Id = 2, Name = "Ropa", Description = "Ropa y accesorios", CreatedAt = new DateTime(2024, 1, 1) },
            new Category { Id = 3, Name = "Hogar", Description = "Artículos para el hogar", CreatedAt = new DateTime(2024, 1, 1) },
            new Category { Id = 4, Name = "Libros", Description = "Libros y revistas", CreatedAt = new DateTime(2024, 1, 1) },
            new Category { Id = 5, Name = "Deportes", Description = "Artículos deportivos", CreatedAt = new DateTime(2024, 1, 1) }
        );
    }
} 