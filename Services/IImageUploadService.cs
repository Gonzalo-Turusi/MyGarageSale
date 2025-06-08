namespace MyGarageSale.Services;

public interface IImageUploadService
{
    Task<string> UploadImageAsync(Stream imageStream, string fileName);
    Task<bool> DeleteImageAsync(string imageUrl);
    bool IsConfigured { get; }
} 