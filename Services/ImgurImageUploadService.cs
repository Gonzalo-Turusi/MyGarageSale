using System.Net.Http.Headers;
using System.Text.Json;
using MyGarageSale.Models;

namespace MyGarageSale.Services;

public class ImgurImageUploadService : IImageUploadService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ImgurImageUploadService> _logger;
    private readonly string? _clientId;

    public ImgurImageUploadService(HttpClient httpClient, IConfiguration configuration, ILogger<ImgurImageUploadService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _clientId = _configuration["Imgur:ClientId"];
    }

    public bool IsConfigured => !string.IsNullOrEmpty(_clientId);

    public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("Imgur Client ID not configured. Please set 'Imgur:ClientId' in your configuration.");
        }

        try
        {
            _logger.LogInformation("🖼️ Subiendo imagen a Imgur: {FileName}", fileName);

            // Preparar el contenido
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(imageStream);
            streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
            content.Add(streamContent, "image", fileName);

            // Configurar headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", _clientId);

            // Hacer la petición
            var response = await _httpClient.PostAsync("https://api.imgur.com/3/upload", content);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("📡 Respuesta de Imgur: Status={StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("❌ Error en Imgur: {StatusCode} - {Content}", response.StatusCode, jsonResponse);
                throw new HttpRequestException($"Imgur API error: {response.StatusCode}");
            }

            // Parsear la respuesta
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var imgurResponse = JsonSerializer.Deserialize<ImgurResponse>(jsonResponse, options);

            if (imgurResponse?.Data?.Link == null)
            {
                _logger.LogError("❌ Respuesta inválida de Imgur: {Response}", jsonResponse);
                throw new Exception("Invalid response from Imgur API");
            }

            var imageUrl = imgurResponse.Data.Link;
            _logger.LogInformation("✅ Imagen subida exitosamente: {Url}", imageUrl);

            return imageUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error subiendo imagen a Imgur: {FileName}", fileName);
            throw new Exception($"Error uploading to Imgur: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteImageAsync(string imageUrl)
    {
        if (!IsConfigured)
        {
            _logger.LogWarning("⚠️ Imgur not configured, cannot delete image: {Url}", imageUrl);
            return false;
        }

        try
        {
            // Extraer el ID de la imagen de la URL
            var imageId = ExtractImageIdFromUrl(imageUrl);
            if (string.IsNullOrEmpty(imageId))
            {
                _logger.LogWarning("⚠️ Cannot extract image ID from URL: {Url}", imageUrl);
                return false;
            }

            _logger.LogInformation("🗑️ Intentando eliminar imagen de Imgur: {ImageId}", imageId);

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", _clientId);

            var response = await _httpClient.DeleteAsync($"https://api.imgur.com/3/image/{imageId}");
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("✅ Imagen eliminada de Imgur: {ImageId}", imageId);
                return true;
            }
            else
            {
                _logger.LogWarning("⚠️ No se pudo eliminar imagen de Imgur: {ImageId} - Status: {StatusCode}", imageId, response.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error eliminando imagen de Imgur: {Url}", imageUrl);
            return false;
        }
    }

    private string? ExtractImageIdFromUrl(string imageUrl)
    {
        try
        {
            // URL format: https://i.imgur.com/ABC123.jpg
            var uri = new Uri(imageUrl);
            var fileName = Path.GetFileNameWithoutExtension(uri.AbsolutePath);
            return fileName?.Replace("/", "");
        }
        catch
        {
            return null;
        }
    }
} 