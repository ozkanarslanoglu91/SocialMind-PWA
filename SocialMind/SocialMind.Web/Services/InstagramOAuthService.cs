using System.Net.Http.Json;
using System.Text.Json;

namespace SocialMind.Web.Services;

public class InstagramOAuthService : IOAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<InstagramOAuthService> _logger;

    private const string AuthorizeEndpoint = "https://api.instagram.com/oauth/authorize";
    private const string TokenEndpoint = "https://graph.instagram.com/v18.0/access_token";
    private const string MeEndpoint = "https://graph.instagram.com/me";

    public InstagramOAuthService(HttpClient httpClient, IConfiguration config, ILogger<InstagramOAuthService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    public string GetAuthorizationUrl(string platform, string userId)
    {
        if (platform != "instagram") throw new ArgumentException("Platform must be 'instagram'");

        var clientId = _config["SocialPlatforms:Instagram:ClientId"];
        var redirectUri = _config["SocialPlatforms:Instagram:RedirectUri"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri))
            throw new InvalidOperationException("Instagram credentials not configured");

        var state = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(userId));

        return $"{AuthorizeEndpoint}?" +
            $"client_id={Uri.EscapeDataString(clientId)}&" +
            $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
            $"scope=user_profile,instagram_basic&" +
            $"response_type=code&" +
            $"state={Uri.EscapeDataString(state)}";
    }

    public async Task<OAuthTokenResponse> ExchangeCodeForTokenAsync(string platform, string code)
    {
        if (platform != "instagram") throw new ArgumentException("Platform must be 'instagram'");

        var clientId = _config["SocialPlatforms:Instagram:ClientId"];
        var clientSecret = _config["SocialPlatforms:Instagram:ClientSecret"];
        var redirectUri = _config["SocialPlatforms:Instagram:RedirectUri"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(redirectUri))
            throw new InvalidOperationException("Instagram credentials not configured");

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

            if (json.TryGetProperty("access_token", out var tokenProp) &&
                json.TryGetProperty("user_id", out var userIdProp))
            {
                return new OAuthTokenResponse
                {
                    AccessToken = tokenProp.GetString() ?? string.Empty,
                    ExpiresAt = DateTime.UtcNow.AddDays(60), // Instagram short-lived tokens
                    Metadata = new Dictionary<string, string>
                    {
                        { "user_id", userIdProp.GetString() ?? string.Empty }
                    }
                };
            }

            throw new InvalidOperationException("Failed to extract token from response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Instagram token exchange failed for code: {Code}", code);
            throw;
        }
    }

    public async Task<OAuthTokenResponse?> RefreshTokenAsync(string platform, string refreshToken)
    {
        // Instagram uses short-lived tokens, need to refresh via long-lived token exchange
        throw new NotImplementedException("Use GetLongLivedTokenAsync instead");
    }

    public async Task<OAuthProfileResponse> GetProfileAsync(string platform, string accessToken)
    {
        if (platform != "instagram") throw new ArgumentException("Platform must be 'instagram'");

        try
        {
            var url = $"{MeEndpoint}?fields=id,username,name,profile_picture_url,biography&access_token={Uri.EscapeDataString(accessToken)}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<JsonElement>(content);

            return new OAuthProfileResponse
            {
                Id = json.GetProperty("id").GetString() ?? string.Empty,
                Username = json.TryGetProperty("username", out var usernameProp) ? usernameProp.GetString() : null,
                DisplayName = json.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null,
                ProfileImageUrl = json.TryGetProperty("profile_picture_url", out var picProp) ? picProp.GetString() : null,
                Bio = json.TryGetProperty("biography", out var bioProp) ? bioProp.GetString() : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Instagram profile fetch failed");
            throw;
        }
    }
}
