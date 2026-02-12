using System.Net.Http.Json;
using System.Text.Json;

namespace SocialMind.Web.Services;

public class YouTubeOAuthService : IOAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<YouTubeOAuthService> _logger;

    private const string AuthorizeEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
    private const string MeEndpoint = "https://www.googleapis.com/youtube/v3/channels";

    public YouTubeOAuthService(HttpClient httpClient, IConfiguration config, ILogger<YouTubeOAuthService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    public string GetAuthorizationUrl(string platform, string userId)
    {
        if (platform != "youtube") throw new ArgumentException("Platform must be 'youtube'");

        var clientId = _config["SocialPlatforms:YouTube:ClientId"];
        var redirectUri = _config["SocialPlatforms:YouTube:RedirectUri"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri))
            throw new InvalidOperationException("YouTube credentials not configured");

        var state = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(userId));

        return $"{AuthorizeEndpoint}?" +
            $"client_id={Uri.EscapeDataString(clientId)}&" +
            $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
            $"response_type=code&" +
            $"scope={Uri.EscapeDataString("https://www.googleapis.com/auth/youtube")}&" +
            $"access_type=offline&" +
            $"prompt=consent&" +
            $"state={Uri.EscapeDataString(state)}";
    }

    public async Task<OAuthTokenResponse> ExchangeCodeForTokenAsync(string platform, string code)
    {
        if (platform != "youtube") throw new ArgumentException("Platform must be 'youtube'");

        var clientId = _config["SocialPlatforms:YouTube:ClientId"];
        var clientSecret = _config["SocialPlatforms:YouTube:ClientSecret"];
        var redirectUri = _config["SocialPlatforms:YouTube:RedirectUri"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(redirectUri))
            throw new InvalidOperationException("YouTube credentials not configured");

        var request = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", redirectUri }
        };

        try
        {
            var response = await _httpClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(request));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<JsonElement>(content);

            if (json.TryGetProperty("access_token", out var tokenProp))
            {
                var expiresIn = json.TryGetProperty("expires_in", out var expiresProp)
                    ? expiresProp.GetInt32()
                    : 3600;

                var refreshToken = json.TryGetProperty("refresh_token", out var refreshProp)
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
            _logger.LogError(ex, "YouTube token exchange failed for code: {Code}", code);
            throw;
        }
    }

    public async Task<OAuthTokenResponse?> RefreshTokenAsync(string platform, string refreshToken)
    {
        if (platform != "youtube") throw new ArgumentException("Platform must be 'youtube'");

        var clientId = _config["SocialPlatforms:YouTube:ClientId"];
        var clientSecret = _config["SocialPlatforms:YouTube:ClientSecret"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            throw new InvalidOperationException("YouTube credentials not configured");

        var request = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken }
        };

        try
        {
            var response = await _httpClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(request));
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<JsonElement>(content);

            if (json.TryGetProperty("access_token", out var tokenProp))
            {
                var expiresIn = json.TryGetProperty("expires_in", out var expiresProp)
                    ? expiresProp.GetInt32()
                    : 3600;

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
            _logger.LogError(ex, "YouTube token refresh failed");
        }

        return null;
    }

    public async Task<OAuthProfileResponse> GetProfileAsync(string platform, string accessToken)
    {
        if (platform != "youtube") throw new ArgumentException("Platform must be 'youtube'");

        try
        {
            var url = $"{MeEndpoint}?part=snippet&mine=true&access_token={Uri.EscapeDataString(accessToken)}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<JsonElement>(content);

            if (json.TryGetProperty("items", out var itemsProp) && itemsProp.GetArrayLength() > 0)
            {
                var channel = itemsProp[0];
                var snippet = channel.GetProperty("snippet");

                return new OAuthProfileResponse
                {
                    Id = channel.GetProperty("id").GetString() ?? string.Empty,
                    DisplayName = snippet.TryGetProperty("title", out var titleProp) ? titleProp.GetString() : null,
                    ProfileImageUrl = snippet.TryGetProperty("thumbnails", out var thumbProp) &&
                                     thumbProp.TryGetProperty("default", out var defaultThumb) &&
                                     defaultThumb.TryGetProperty("url", out var urlProp)
                        ? urlProp.GetString()
                        : null,
                    Bio = snippet.TryGetProperty("description", out var descProp) ? descProp.GetString() : null
                };
            }

            throw new InvalidOperationException("No YouTube channel found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "YouTube profile fetch failed");
            throw;
        }
    }
}
