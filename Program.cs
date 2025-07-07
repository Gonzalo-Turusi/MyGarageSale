using Microsoft.EntityFrameworkCore;
using MyGarageSale.Components;
using MyGarageSale.Data;
using MyGarageSale.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Entity Framework - Usar Azure SQL en producción, SQLite en desarrollo
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (builder.Environment.IsProduction())
    {
        // En producción usar Azure SQL
        var connectionString = builder.Configuration.GetConnectionString("AzureSqlConnection") ??
                             throw new InvalidOperationException("La cadena de conexión 'AzureSqlConnection' no está configurada.");
        options.UseSqlServer(connectionString);
    }
    else
    {
        // En desarrollo usar SQLite
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                         "Data Source=garage_sale.db");
    }
});

// Register application services
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IPurchaseRequestService, PurchaseRequestService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISeedDataService, SeedDataService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICartService, CartService>();

// Add HTTP client for file uploads
builder.Services.AddHttpClient();

// Register image upload services
// Primero registramos ambos servicios
builder.Services.AddScoped<ImgurImageUploadService>();
builder.Services.AddScoped<LocalImageUploadService>();

// Luego registramos la interfaz con una factory que decide cuál usar
builder.Services.AddScoped<IImageUploadService>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    
    // Preferir Imgur si está configurado
    var imgurClientId = configuration["Imgur:ClientId"];
    if (!string.IsNullOrEmpty(imgurClientId))
    {
        logger.LogInformation("🖼️ Usando Imgur para almacenamiento de imágenes");
        return serviceProvider.GetRequiredService<ImgurImageUploadService>();
    }
    else
    {
        logger.LogInformation("💾 Usando almacenamiento local para imágenes (Imgur no configurado)");
        return serviceProvider.GetRequiredService<LocalImageUploadService>();
    }
});

var app = builder.Build();

// Ensure database is created, migrations applied, and seed sample data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Apply any pending migrations safely (this won't delete data)
    try
    {
        context.Database.Migrate();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("✅ Migraciones aplicadas exitosamente");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ Error aplicando migraciones: {Message}", ex.Message);
        // Fallback: ensure database exists
        context.Database.EnsureCreated();
    }
    
    // Seed sample items if none exist
    var seedService = scope.ServiceProvider.GetRequiredService<ISeedDataService>();
    await seedService.SeedSampleItemsAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
