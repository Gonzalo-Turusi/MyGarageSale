using System.Text.Json.Serialization;

namespace MyGarageSale.Models;

public class ImgurResponse
{
    [JsonPropertyName("data")]
    public ImgurData? Data { get; set; }
    
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("status")]
    public int Status { get; set; }
}

public class ImgurData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("link")]
    public string Link { get; set; } = string.Empty;
    
    [JsonPropertyName("deletehash")]
    public string? DeleteHash { get; set; }
    
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    
    [JsonPropertyName("width")]
    public int? Width { get; set; }
    
    [JsonPropertyName("height")]
    public int? Height { get; set; }
    
    [JsonPropertyName("size")]
    public long? Size { get; set; }
} 