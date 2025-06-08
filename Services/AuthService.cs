using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Configuration;

namespace MyGarageSale.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string username, string password);
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
}

public class AuthService : IAuthService
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;
    private const string AuthKey = "isAuthenticated";

    public AuthService(ProtectedSessionStorage sessionStorage, ILogger<AuthService> logger, IConfiguration configuration)
    {
        _sessionStorage = sessionStorage;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            // Leer credenciales desde configuración con fallback a valores por defecto
            var adminUsername = _configuration["AdminCredentials:Username"] ?? "admin";
            var adminPassword = _configuration["AdminCredentials:Password"] ?? "admin123";
            
            _logger.LogInformation("🔐 Verificando credenciales: user={Username}, expected={ExpectedUser}", 
                username?.Trim(), adminUsername);
            
            if (username?.Trim().ToLower() == adminUsername.ToLower() && password == adminPassword)
            {
                await _sessionStorage.SetAsync(AuthKey, true);
                _logger.LogInformation("✅ Administrator logged in successfully");
                return true;
            }
            
            _logger.LogWarning("❌ Failed login attempt for username: {Username}", username);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error during login process");
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            await _sessionStorage.DeleteAsync(AuthKey);
            _logger.LogInformation("Administrator logged out");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout process");
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            var result = await _sessionStorage.GetAsync<bool>(AuthKey);
            return result.Success && result.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking authentication status");
            return false;
        }
    }
} 