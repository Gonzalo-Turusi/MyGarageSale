using MyGarageSale.Data;
using MyGarageSale.Models;
using Microsoft.EntityFrameworkCore;

namespace MyGarageSale.Services;

public interface ISeedDataService
{
    Task SeedSampleItemsAsync();
}

public class SeedDataService : ISeedDataService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SeedDataService> _logger;

    public SeedDataService(AppDbContext context, ILogger<SeedDataService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedSampleItemsAsync()
    {
        try
        {
            // Verificar si ya hay items (evitar duplicados)
            if (_context.Items.Any())
            {
                _logger.LogInformation("Items already exist, skipping seed data");
                return;
            }

            var categories = await _context.Categories.ToListAsync();
            if (!categories.Any())
            {
                _logger.LogWarning("No categories found, cannot seed items");
                return;
            }

            var electronicsCategory = categories.First(c => c.Name == "Electrónicos");
            var clothingCategory = categories.First(c => c.Name == "Ropa");
            var homeCategory = categories.First(c => c.Name == "Hogar");
            var booksCategory = categories.First(c => c.Name == "Libros");
            var sportsCategory = categories.First(c => c.Name == "Deportes");

            var sampleItems = new List<Item>
            {
                // Electrónicos
                new Item
                {
                    Title = "iPhone 12 Pro - Usado",
                    Description = "iPhone 12 Pro de 128GB en excelente estado. Incluye cargador original y funda protectora. Sin rayones en pantalla.",
                    Price = 420.00m,
                    CategoryId = electronicsCategory.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    IsSold = false
                },
                new Item
                {
                    Title = "PlayStation 4 Pro 1TB",
                    Description = "Consola PlayStation 4 Pro con 1TB de almacenamiento. Incluye 2 mandos y 5 juegos: FIFA 23, Call of Duty, Spider-Man, God of War y The Last of Us Part II.",
                    Price = 280.00m,
                    CategoryId = electronicsCategory.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-8),
                    IsSold = true // VENDIDO - para mostrar visualmente
                },
                new Item
                {
                    Title = "MacBook Air M1 2020",
                    Description = "MacBook Air con chip M1, 8GB RAM, 256GB SSD. En perfectas condiciones, poco uso. Ideal para trabajo y estudio.",
                    Price = 850.00m,
                    CategoryId = electronicsCategory.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    IsSold = false
                },

                // Ropa
                new Item
                {
                    Title = "Chaqueta de Cuero Negra - Talla M",
                    Description = "Chaqueta de cuero genuino marca Zara, talla M. En excelente estado, solo usada pocas veces. Muy elegante y versátil.",
                    Price = 45.00m,
                    CategoryId = clothingCategory.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-12),
                    IsSold = true // VENDIDO - para mostrar visualmente
                },
                new Item
                {
                    Title = "Zapatos Nike Air Force 1 - Talla 42",
                    Description = "Zapatillas Nike Air Force 1 blancas, talla 42. En muy buen estado, con signos mínimos de uso. Cómodas y clásicas.",
                    Price = 35.00m,
                    CategoryId = clothingCategory.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    IsSold = false
                },

                // Hogar
                new Item
                {
                    Title = "Mesa de Centro de Madera",
                    Description = "Hermosa mesa de centro de madera maciza. Perfecta para sala de estar. Dimensiones: 120cm x 60cm x 45cm de alto.",
                    Price = 120.00m,
                    CategoryId = homeCategory.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    IsSold = false
                },
                new Item
                {
                    Title = "Set de Sartenes Antiadherentes",
                    Description = "Juego de 3 sartenes antiadherentes de diferentes tamaños. Marca Tefal, en excelente estado. Ideales para cocina moderna.",
                    Price = 25.00m,
                    CategoryId = homeCategory.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    IsSold = false
                },
                new Item
                {
                    Title = "Lámpara de Pie Moderna",
                    Description = "Lámpara de pie con diseño moderno y minimalista. Base de metal negro y pantalla de tela blanca. Perfecta para iluminar cualquier rincón.",
                    Price = 60.00m,
                    CategoryId = homeCategory.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-6),
                    IsSold = false
                },

                // Libros
                new Item
                {
                    Title = "Colección Harry Potter Completa",
                    Description = "Los 7 libros de Harry Potter en español, edición de tapa dura. En perfecto estado, prácticamente nuevos. Una joya para cualquier fan.",
                    Price = 80.00m,
                    CategoryId = booksCategory.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    IsSold = true // VENDIDO - para mostrar visualmente  
                },
                new Item
                {
                    Title = "Libros de Programación - Pack de 5",
                    Description = "Pack de 5 libros sobre programación: JavaScript, Python, C#, React y Bases de Datos. Perfectos para aprender o como referencia.",
                    Price = 40.00m,
                    CategoryId = booksCategory.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-4),
                    IsSold = false
                },

                // Deportes
                new Item
                {
                    Title = "Bicicleta de Montaña Trek",
                    Description = "Bicicleta de montaña Trek en excelente estado. 21 velocidades, suspensión delantera. Ideal para rutas y ejercicio. Incluye casco.",
                    Price = 320.00m,
                    CategoryId = sportsCategory.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-18),
                    IsSold = false
                },
                new Item
                {
                    Title = "Set de Pesas Ajustables",
                    Description = "Juego de pesas ajustables de 2 a 24kg cada una. Perfecto para entrenamiento en casa. Incluye barras y discos adicionales.",
                    Price = 150.00m,
                    CategoryId = sportsCategory.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-9),
                    IsSold = false
                },
                new Item
                {
                    Title = "Reloj Rolex Oyster Perpetual",
                    Description = "Reloj Rolex Oyster Perpetual de oro rosa con dial azul. En excelente estado, con caja y correa de cuero.",
                    Price = 1500.00m,
                    CategoryId = electronicsCategory.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    IsSold = false
                }
            };

            await _context.Items.AddRangeAsync(sampleItems);
            await _context.SaveChangesAsync();

            // Agregar imágenes de muestra para algunos artículos
            await SeedSampleImagesAsync();

            _logger.LogInformation("Successfully seeded {Count} sample items", sampleItems.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding sample items");
            throw;
        }
    }

    private async Task SeedSampleImagesAsync()
    {
        var items = await _context.Items.ToListAsync();
        var itemImages = new List<ItemImage>();

        // iPhone 12 Pro - múltiples ángulos
        var iPhone = items.FirstOrDefault(i => i.Title.Contains("iPhone 12 Pro"));
        if (iPhone != null)
        {
            itemImages.AddRange(new[]
            {
                new ItemImage { ItemId = iPhone.Id, ImagePath = "https://images.unsplash.com/photo-1592899677977-9c10ca588bbd?w=400&h=400&fit=crop", AltText = "iPhone 12 Pro vista frontal", Order = 1 },
                new ItemImage { ItemId = iPhone.Id, ImagePath = "https://images.unsplash.com/photo-1574944985070-8f3ebc6b79d2?w=400&h=400&fit=crop", AltText = "iPhone 12 Pro vista trasera", Order = 2 },
                new ItemImage { ItemId = iPhone.Id, ImagePath = "https://images.unsplash.com/photo-1512499617640-c74ae3a79d37?w=400&h=400&fit=crop", AltText = "iPhone 12 Pro vista lateral", Order = 3 },
                new ItemImage { ItemId = iPhone.Id, ImagePath = "https://images.unsplash.com/photo-1510557880182-3d4d3cba35a5?w=400&h=400&fit=crop", AltText = "iPhone 12 Pro con accesorios incluidos", Order = 4 }
            });
        }

        // PlayStation 4 Pro - consola y juegos
        var ps4 = items.FirstOrDefault(i => i.Title.Contains("PlayStation 4 Pro"));
        if (ps4 != null)
        {
            itemImages.AddRange(new[]
            {
                new ItemImage { ItemId = ps4.Id, ImagePath = "https://images.unsplash.com/photo-1493711662062-fa541adb3fc8?w=400&h=400&fit=crop", AltText = "PlayStation 4 Pro consola", Order = 1 },
                new ItemImage { ItemId = ps4.Id, ImagePath = "https://images.unsplash.com/photo-1511512578047-dfb367046420?w=400&h=400&fit=crop", AltText = "PlayStation 4 Pro con mandos", Order = 2 },
                new ItemImage { ItemId = ps4.Id, ImagePath = "https://images.unsplash.com/photo-1606144042614-b2417e99c4e3?w=400&h=400&fit=crop", AltText = "Juegos incluidos con PS4", Order = 3 }
            });
        }

        // Bicicleta de Montaña - diferentes ángulos
        var bike = items.FirstOrDefault(i => i.Title.Contains("Bicicleta de Montaña"));
        if (bike != null)
        {
            itemImages.AddRange(new[]
            {
                new ItemImage { ItemId = bike.Id, ImagePath = "https://images.unsplash.com/photo-1544191696-15693072c637?w=400&h=400&fit=crop", AltText = "Bicicleta de montaña Trek vista completa", Order = 4 },
                new ItemImage { ItemId = bike.Id, ImagePath = "https://images.unsplash.com/photo-1493836512294-502baa1986e2?w=400&h=400&fit=crop", AltText = "Detalle de ruedas y suspensión delantera", Order = 2 },
                new ItemImage { ItemId = bike.Id, ImagePath = "https://images.unsplash.com/photo-1502744688674-6e0eb8cfb237?w=400&h=400&fit=crop", AltText = "Sistema de cambios y manillar", Order = 3 },
                new ItemImage { ItemId = bike.Id, ImagePath = "https://images.unsplash.com/photo-1571068316344-75bc76f77890?w=400&h=400&fit=crop", AltText = "Casco de ciclista incluido", Order = 1 }
            });
        }

        // Mesa de Centro - diferentes vistas
        var mesa = items.FirstOrDefault(i => i.Title.Contains("Mesa de Centro"));
        if (mesa != null)
        {
            itemImages.AddRange(new[]
            {
                new ItemImage { ItemId = mesa.Id, ImagePath = "https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=400&h=400&fit=crop", AltText = "Mesa de centro vista frontal", Order = 1 },
                new ItemImage { ItemId = mesa.Id, ImagePath = "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=400&h=400&fit=crop", AltText = "Mesa de centro vista superior", Order = 2 },
                new ItemImage { ItemId = mesa.Id, ImagePath = "https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=400&h=400&fit=crop", AltText = "Detalle de la madera", Order = 3 }
            });
        }

        // Colección Harry Potter
        var harryPotter = items.FirstOrDefault(i => i.Title.Contains("Harry Potter"));
        if (harryPotter != null)
        {
            itemImages.AddRange(new[]
            {
                new ItemImage { ItemId = harryPotter.Id, ImagePath = "https://images.unsplash.com/photo-1481627834876-b7833e8f5570?w=400&h=400&fit=crop", AltText = "Colección completa Harry Potter", Order = 1 },
                new ItemImage { ItemId = harryPotter.Id, ImagePath = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=400&h=400&fit=crop", AltText = "Libros individuales", Order = 2 }
            });
        }

        // Agregar imágenes para más productos
        var chaqueta = items.FirstOrDefault(i => i.Title.Contains("Chaqueta de Cuero"));
        if (chaqueta != null)
        {
            itemImages.AddRange(new[]
            {
                new ItemImage { ItemId = chaqueta.Id, ImagePath = "https://images.unsplash.com/photo-1551028719-00167b16eac5?w=400&h=400&fit=crop", AltText = "Chaqueta de cuero vista frontal", Order = 1 },
                new ItemImage { ItemId = chaqueta.Id, ImagePath = "https://images.unsplash.com/photo-1544966503-7cc5ac882d7a?w=400&h=400&fit=crop", AltText = "Chaqueta de cuero vista trasera", Order = 2 },
                new ItemImage { ItemId = chaqueta.Id, ImagePath = "https://images.unsplash.com/photo-1520975954732-35dd22299614?w=400&h=400&fit=crop", AltText = "Detalle de cremalleras y acabados", Order = 3 }
            });
        }

        var airForce = items.FirstOrDefault(i => i.Title.Contains("Nike Air Force"));
        if (airForce != null)
        {
            itemImages.AddRange(new[]
            {
                new ItemImage { ItemId = airForce.Id, ImagePath = "https://images.unsplash.com/photo-1549298916-b41d501d3772?w=400&h=400&fit=crop", AltText = "Nike Air Force 1 vista lateral", Order = 1 },
                new ItemImage { ItemId = airForce.Id, ImagePath = "https://images.unsplash.com/photo-1600185365483-26d7a4cc7519?w=400&h=400&fit=crop", AltText = "Nike Air Force 1 vista superior", Order = 2 },
                new ItemImage { ItemId = airForce.Id, ImagePath = "https://images.unsplash.com/photo-1595950653106-6c9ebd614d3a?w=400&h=400&fit=crop", AltText = "Nike Air Force 1 suela y detalle", Order = 3 },
                new ItemImage { ItemId = airForce.Id, ImagePath = "https://images.unsplash.com/photo-1560769629-975ec94e6a86?w=400&h=400&fit=crop", AltText = "Par de zapatillas", Order = 4 }
            });
        }

        var lampara = items.FirstOrDefault(i => i.Title.Contains("Lámpara de Pie"));
        if (lampara != null)
        {
            itemImages.AddRange(new[]
            {
                new ItemImage { ItemId = lampara.Id, ImagePath = "https://images.unsplash.com/photo-1567538096630-e0c55bd6374c?w=400&h=400&fit=crop", AltText = "Lámpara de pie encendida", Order = 1 },
                new ItemImage { ItemId = lampara.Id, ImagePath = "https://images.unsplash.com/photo-1513506003901-1e6a229e2d15?w=400&h=400&fit=crop", AltText = "Lámpara de pie apagada", Order = 2 },
                new ItemImage { ItemId = lampara.Id, ImagePath = "https://images.unsplash.com/photo-1524484485831-a92ffc0de03f?w=400&h=400&fit=crop", AltText = "Detalle de la base y materiales", Order = 3 }
            });
        }

        // Reloj Rolex - múltiples ángulos y detalles
        var reloj = items.FirstOrDefault(i => i.Title.Contains("Reloj Rolex"));
        if (reloj != null)
        {
            itemImages.AddRange(new[]
            {
                new ItemImage { ItemId = reloj.Id, ImagePath = "https://images.unsplash.com/photo-1600003014755-ba31aa59c4b6?w=400&h=400&fit=crop", AltText = "Reloj Rolex vista completa", Order = 1 },
                new ItemImage { ItemId = reloj.Id, ImagePath = "https://images.unsplash.com/photo-1599391817606-84b5f3c86a8c?w=400&h=400&fit=crop", AltText = "Detalle del cordaje y marco", Order = 2 },
                new ItemImage { ItemId = reloj.Id, ImagePath = "https://images.unsplash.com/photo-1600003014755-ba31aa59c4b6?w=400&h=400&fit=crop&sat=-50", AltText = "Grip y mango de la raqueta", Order = 3 },
                new ItemImage { ItemId = reloj.Id, ImagePath = "https://images.unsplash.com/photo-1544737151131-6e4999de2fb5?w=400&h=400&fit=crop", AltText = "Raqueta con pelotas de tenis", Order = 4 }
            });
        }

        var macbook = items.FirstOrDefault(i => i.Title.Contains("MacBook Air"));
        if (macbook != null)
        {
            itemImages.AddRange(new[]
            {
                new ItemImage { ItemId = macbook.Id, ImagePath = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=400&h=400&fit=crop", AltText = "MacBook Air cerrado", Order = 1 },
                new ItemImage { ItemId = macbook.Id, ImagePath = "https://images.unsplash.com/photo-1541807084-5c52b6b3adef?w=400&h=400&fit=crop", AltText = "MacBook Air abierto mostrando pantalla", Order = 2 },
                new ItemImage { ItemId = macbook.Id, ImagePath = "https://images.unsplash.com/photo-1484788984921-03950022c9ef?w=400&h=400&fit=crop", AltText = "MacBook Air vista lateral mostrando grosor", Order = 3 }
            });
        }

        var sartenes = items.FirstOrDefault(i => i.Title.Contains("Sartenes Antiadherentes"));
        if (sartenes != null)
        {
            itemImages.AddRange(new[]
            {
                new ItemImage { ItemId = sartenes.Id, ImagePath = "https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=400&h=400&fit=crop", AltText = "Set completo de sartenes", Order = 1 },
                new ItemImage { ItemId = sartenes.Id, ImagePath = "https://images.unsplash.com/photo-1556909195-4e4d598511de?w=400&h=400&fit=crop", AltText = "Sartén individual mostrando recubrimiento", Order = 2 }
            });
        }

        var librosProgramacion = items.FirstOrDefault(i => i.Title.Contains("Libros de Programación"));
        if (librosProgramacion != null)
        {
            itemImages.AddRange(new[]
            {
                new ItemImage { ItemId = librosProgramacion.Id, ImagePath = "https://images.unsplash.com/photo-1555066931-4365d14bab8c?w=400&h=400&fit=crop", AltText = "Pack de libros de programación", Order = 1 },
                new ItemImage { ItemId = librosProgramacion.Id, ImagePath = "https://images.unsplash.com/photo-1516979187457-637abb4f9353?w=400&h=400&fit=crop", AltText = "Libros individuales mostrando títulos", Order = 2 },
                new ItemImage { ItemId = librosProgramacion.Id, ImagePath = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=400&h=400&fit=crop", AltText = "Contenido y códigos de ejemplo", Order = 3 }
            });
        }

        var pesas = items.FirstOrDefault(i => i.Title.Contains("Pesas Ajustables"));
        if (pesas != null)
        {
            itemImages.AddRange(new[]
            {
                new ItemImage { ItemId = pesas.Id, ImagePath = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400&h=400&fit=crop", AltText = "Set completo de pesas ajustables", Order = 1 },
                new ItemImage { ItemId = pesas.Id, ImagePath = "https://images.unsplash.com/photo-1434682881908-b43d0467b798?w=400&h=400&fit=crop", AltText = "Discos y barras adicionales", Order = 2 },
                new ItemImage { ItemId = pesas.Id, ImagePath = "https://images.unsplash.com/photo-1517963628607-235ccdd5476c?w=400&h=400&fit=crop", AltText = "Pesas en uso mostrando diferentes pesos", Order = 3 }
            });
        }

        if (itemImages.Any())
        {
        await _context.ItemImages.AddRangeAsync(itemImages);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Successfully seeded {Count} sample images", itemImages.Count);
        }
    }
} 