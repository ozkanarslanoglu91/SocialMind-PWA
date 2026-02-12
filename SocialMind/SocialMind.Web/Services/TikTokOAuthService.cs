using System.Net.Http.Json;
using System.Text.Json;

namespace SocialMind.Web.Services;

public class TikTokOAuthService : IOAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<TikTokOAuthService> _logger;

    private const string AuthorizeEndpoint = "https://www.tiktok.com/v1/oauth/authorize";
    private const string TokenEndpoint = "https://open.tiktokapis.com/v1/oauth/token";
    private const string MeEndpoint = "https://open.tiktokapis.com/v1/user/info";

    public TikTokOAuthService(HttpClient httpClient, IConfiguration config, ILogger<TikTokOAuthService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    public string GetAuthorizationUrl(string platform, string userId)
    {
        if (platform != "tiktok") throw new ArgumentException("Platform must be 'tiktok'");

        var clientId = _config["SocialPlatforms:TikTok:ClientId"];
        var redirectUri = _config["SocialPlatforms:TikTok:RedirectUri"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri))
            throw new InvalidOperationException("TikTok credentials not configured");

        var state = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(userId));

        return $"{AuthorizeEndpoint}?" +
            $"client_key={Uri.EscapeDataString(clientId)}&" +
            $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
            $"scope={Uri.EscapeDataString("user.info.basic")}&" +
            $"response_type=code&" +
            $"state={Uri.EscapeDataString(state)}";
    }

    public async Task<OAuthTokenResponse> ExchangeCodeForTokenAsync(string platform, string code)
    {
        if (platform != "tiktok") throw new ArgumentException("Platform must be 'tiktok'");

        var clientId = _config["SocialPlatforms:TikTok:ClientId"];
        var clientSecret = _config["SocialPlatforms:TikTok:ClientSecret"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            throw new InvalidOperationException("TikTok credentials not configured");

        var request = new
        {
            client_key = clientId,
            client_secret = clientSecret,
            code,
            grant_type = "authorization_code"
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(TokenEndpoint, request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<JsonElement>(content);

            if (json.TryGetProperty("data", out var dataProp) &&
                dataProp.TryGetProperty("access_token", out var tokenProp))
            {
                var expiresIn = dataProp.TryGetProperty("expires_in", out var expiresProp)
                    ? expiresProp.GetInt32()
                    : 7200;

                var refreshToken = dataProp.TryGetProperty("refresh_token", out var refreshProp)
                    ? refreshProp.GetString()
                    : null;

                return new OAuthTokenResponse
                {
                    AccessToken = tokenProp.GetString() ?? string.Empty,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn)
                };
            }

            throw new InvalidOperationException("Failed to extract token from response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TikTok token exchange failed for code: {Code}", code);
            throw;
        }
    }

    public async Task<OAuthTokenResponse?> RefreshTokenAsync(string platform, string refreshToken)
    {
        if (platform != "tiktok") throw new ArgumentException("Platform must be 'tiktok'");

        var clientId = _config["SocialPlatforms:TikTok:ClientId"];
        var clientSecret = _config["SocialPlatforms:TikTok:ClientSecret"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            throw new InvalidOperationException("TikTok credentials not configured");

        var request = new
        {
            client_key = clientId,
            client_secret = clientSecret,
            refresh_token = refreshToken,
            grant_type = "refresh_token"
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(TokenEndpoint, request);
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<JsonElement>(content);

            if (json.TryGetProperty("data", out var dataProp) &&
                dataProp.TryGetProperty("access_token", out var tokenProp))
            {
                var expiresIn = dataProp.TryGetProperty("expires_in", out var expiresProp)
                    ? expiresProp.GetInt32()
                    : 7200;

                return new OAuthTokenResponse
                {
                    AccessToken = tokenProp.GetString() ?? string.Empty,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn)
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TikTok token refresh failed");
        }

        return null;
    }

    public async Task<OAuthProfileResponse> GetProfileAsync(string platform, string accessToken)
    {
        if (platform != "tiktok") throw new ArgumentException("Platform must be 'tiktok'");

        try
        {
            var url = $"{MeEndpoint}?fields=open_id,display_name,avatar_large_url";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<JsonElement>(content);

            if (json.TryGetProperty("data", out var dataProp))
            {
                dataProp.TryGetProperty("user", out var userProp);

                return new OAuthProfileResponse
                {
                    Id = userProp.TryGetProperty("open_id", out var idProp) ? idProp.GetString() ?? string.Empty : string.Empty,
                    DisplayName = userProp.TryGetProperty("display_name", out var displayProp) ? displayProp.GetString() : null,
                    ProfileImageUrl = userProp.TryGetProperty("avatar_large_url", out var avatarProp) ? avatarProp.GetString() : null
                };
            }

            throw new InvalidOperationException("Invalid TikTok profile response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TikTok profile fetch failed");
            throw;
        }
    }
}
