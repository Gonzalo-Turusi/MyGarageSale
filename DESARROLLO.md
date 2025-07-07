# 👨‍💻 Guía de Desarrollo - MyGarageSale

## 📋 Índice
- [Arquitectura](#arquitectura)
- [Estructura de Código](#estructura-de-código)
- [Patrones de Diseño](#patrones-de-diseño)
- [Servicios y Dependencias](#servicios-y-dependencias)
- [Frontend y UI](#frontend-y-ui)
- [Base de Datos](#base-de-datos)
- [Testing](#testing)
- [Performance](#performance)
- [Deployment](#deployment)

---

## 🏗️ Arquitectura

### **Patrón: Clean Architecture + DDD Simplificado**

La aplicación sigue una arquitectura en capas con separación clara de responsabilidades:

- **Presentation Layer**: Componentes Blazor
- **Business Logic Layer**: Services e Interfaces
- **Data Access Layer**: Entity Framework + DbContext
- **External Services**: APIs (Imgur, SMTP)

### **Tecnologías Core**
- **.NET 9**: Framework principal
- **Blazor Server**: SPA con SignalR
- **Entity Framework Core**: ORM
- **SQLite/Azure SQL**: Base de datos
- **Tailwind CSS**: Styling
- **Imgur API**: Almacenamiento de imágenes

---

## 📁 Estructura de Código

### **Organización de Directorios**

```
MyGarageSale/
├── 📂 Components/
│   ├── 📂 Admin/           # Panel administrativo
│   ├── 📂 Layout/          # Layouts compartidos
│   ├── 📂 Pages/           # Páginas principales
│   └── *.razor             # Componentes reutilizables
├── 📂 Services/            # Lógica de negocio
│   ├── I*Service.cs        # Interfaces
│   └── *Service.cs         # Implementaciones
├── 📂 Models/              # Modelos de dominio
├── 📂 Data/                # Acceso a datos
└── Program.cs              # Configuración y DI
```

### **Convenciones de Nomenclatura**

- **Servicios**: `I{Name}Service.cs` + `{Name}Service.cs`
- **Componentes**: `PascalCase.razor`
- **Modelos**: `PascalCase.cs`
- **Variables**: `camelCase` (C#), `kebab-case` (CSS)

---

## 🎯 Patrones de Diseño

### **1. Service Layer Pattern**
```csharp
public interface IItemService
{
    Task<IEnumerable<Item>> GetAllItemsAsync();
    Task<Item?> GetItemByIdAsync(int id);
    Task<Item> CreateItemAsync(Item item);
}

builder.Services.AddScoped<IItemService, ItemService>();
```

### **2. Strategy Pattern (Image Upload)**
```csharp
builder.Services.AddScoped<IImageUploadService>(serviceProvider =>
{
    var imgurClientId = configuration["Imgur:ClientId"];
    return !string.IsNullOrEmpty(imgurClientId)
        ? serviceProvider.GetRequiredService<ImgurImageUploadService>()
        : serviceProvider.GetRequiredService<LocalImageUploadService>();
});
```

### **3. Configuration Pattern (Email Service)**
```csharp
private async Task<bool> SendEmailViaSmtpAsync(string toEmail, string subject, string body)
{
    var smtpHost = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
    var smtpPort = _configuration.GetValue<int>("Smtp:Port", 587);
    var smtpUsername = _configuration["Smtp:Username"];
    var smtpPassword = _configuration["Smtp:Password"];
    
    // Usar configuración SMTP (Gmail)
    using var client = new SmtpClient(smtpHost, smtpPort);
    // ... implementación
}
```

---

## 🔧 Servicios y Dependencias

### **Inyección de Dependencias**

```csharp
// Program.cs - Configuración de servicios
var builder = WebApplication.CreateBuilder(args);

// Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Entity Framework
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (builder.Environment.IsProduction())
        options.UseSqlServer(connectionString);
    else
        options.UseSqlite("Data Source=garage_sale.db");
});

// Application Services
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IPurchaseRequestService, PurchaseRequestService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISeedDataService, SeedDataService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<ICartService, CartService>();
```

### **Lifecycle de Servicios**
- **Scoped**: Servicios de dominio (por request)
- **Singleton**: CartService (estado compartido)
- **Transient**: No usado (evitar overhead)

---

## 🎨 Frontend y UI

### **Estructura de Componente Típico**
```razor
@page "/example"
@inject IExampleService ExampleService
@inject IJSRuntime JSRuntime

<div class="container mx-auto px-4">
    <!-- UI Content -->
</div>

@code {
    [Parameter] public int Id { get; set; }
    
    private List<Item> items = new();
    private bool isLoading = false;
    
    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }
    
    private async Task LoadDataAsync()
    {
        isLoading = true;
        StateHasChanged();
        
        try
        {
            items = await ExampleService.GetItemsAsync();
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }
}
```

### **Design System (Tailwind CSS)**

```css
/* input.css */
@tailwind base;
@tailwind components;
@tailwind utilities;

@layer components {
  .btn-primary {
    @apply bg-blue-600 hover:bg-blue-700 text-white font-semibold py-2 px-4 rounded-lg transition-colors duration-200;
  }
  
  .card {
    @apply bg-white rounded-lg shadow-md overflow-hidden border border-gray-200;
  }
}
```

---

## 🗃️ Base de Datos

### **Modelo de Datos**

```csharp
public class Item
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public bool IsSold { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public ICollection<ItemImage> Images { get; set; } = new List<ItemImage>();
    public ICollection<PurchaseRequest> PurchaseRequests { get; set; } = new List<PurchaseRequest>();
}
```

### **DbContext Configuration**
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Item>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
        entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
        entity.HasOne(e => e.Category)
              .WithMany(c => c.Items)
              .HasForeignKey(e => e.CategoryId);
    });
}
```

---

## ⚡ Performance

### **Optimizaciones Implementadas**

#### **Database Performance**
```csharp
// Eager loading para evitar N+1
var items = await _context.Items
    .Include(i => i.Category)
    .Include(i => i.Images)
    .ToListAsync();
```

#### **Blazor Performance**
```razor
@* Virtualization para listas grandes *@
<Virtualize Items="@items" Context="item">
    <ItemCard Item="@item" />
</Virtualize>

@* Conditional rendering *@
@if (isLoading)
{
    <div class="spinner"></div>
}
else if (items.Any())
{
    <!-- Render items -->
}
```

---

## 🚀 Deployment

### **Environment Configuration**
```csharp
if (builder.Environment.IsProduction())
{
    // Production: Azure SQL
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
}
else
{
    // Development: SQLite
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite("Data Source=garage_sale.db"));
}
```

---

## 📚 Recursos para Desarrolladores

### **Documentación Técnica**
- 🔗 [.NET 9 Documentation](https://docs.microsoft.com/dotnet/core/whats-new/dotnet-9)
- 🔗 [Blazor Server Guide](https://docs.microsoft.com/aspnet/core/blazor)
- 🔗 [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- 🔗 [Tailwind CSS](https://tailwindcss.com/docs)

### **Tools Recomendadas**
- **IDE**: Visual Studio 2022 / VS Code / Cursor
- **Database**: Azure Data Studio
- **API Testing**: Postman / Thunder Client
- **Performance**: dotnet-trace
- **Monitoring**: Application Insights

---

**🚀 Happy Coding!** Esta aplicación está diseñada para ser mantenible, escalable y fácil de entender. 