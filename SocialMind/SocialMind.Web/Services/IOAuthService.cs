namespace SocialMind.Web.Services;

/// <summary>
/// OAuth akışı yönetim arayüzü
/// </summary>
public interface IOAuthService
{
    /// <summary>
    /// Platform'un authorize endpoint'ine yönlendirme URL'si oluştur
    /// </summary>
    string GetAuthorizationUrl(string platform, string userId);

    /// <summary>
    /// Authorization code'u token'a exchange et
    /// </summary>
    Task<OAuthTokenResponse> ExchangeCodeForTokenAsync(string platform, string code);

    /// <summary>
    /// Token'ı refresh et (eğer destekleniyorsa)
    /// </summary>
    Task<OAuthTokenResponse?> RefreshTokenAsync(string platform, string refreshToken);

    /// <summary>
    /// Platform profilini çek
    /// </summary>
    Task<OAuthProfileResponse> GetProfileAsync(string platform, string accessToken);
}

public class OAuthTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public Dictionary<string, string>? Metadata { get; set; } = new();
}

public class OAuthProfileResponse
{
    public string Id { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? DisplayName { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? Bio { get; set; }
    public Dictionary<string, object>? Metadata { get; set; } = new();
}
