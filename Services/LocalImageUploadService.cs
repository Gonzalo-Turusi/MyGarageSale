namespace MyGarageSale.Services;

public class LocalImageUploadService : IImageUploadService
{
    private readonly ILogger<LocalImageUploadService> _logger;
    private readonly IWebHostEnvironment _environment;

    public LocalImageUploadService(ILogger<LocalImageUploadService> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public bool IsConfigured => true; // Siempre disponible como fallback

    public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
    {
        try
        {
            _logger.LogInformation("💾 Guardando imagen localmente: {FileName}", fileName);

            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
            
            // Crear directorio si no existe
            Directory.CreateDirectory(uploadsPath);
            
            var filePath = Path.Combine(uploadsPath, uniqueFileName);
            
            using var fileStream = new FileStream(filePath, FileMode.Create);
            await imageStream.CopyToAsync(fileStream);
            
            var relativePath = $"/uploads/{uniqueFileName}";
            _logger.LogInformation("✅ Imagen guardada localmente: {Path}", relativePath);
            
            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error guardando imagen localmente: {FileName}", fileName);
            throw new Exception($"Error saving image locally: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteImageAsync(string imageUrl)
    {
        try
        {
            if (imageUrl.StartsWith("/uploads/"))
            {
                var filePath = Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/'));
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("✅ Imagen local eliminada: {Path}", imageUrl);
                    return true;
                }
            }
            
            _logger.LogWarning("⚠️ Archivo no encontrado para eliminar: {Path}", imageUrl);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error eliminando imagen local: {Path}", imageUrl);
            return false;
        }
        finally
        {
            await Task.CompletedTask;
        }
    }
} 