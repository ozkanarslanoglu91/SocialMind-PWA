namespace SocialMind.Shared.Models;

/// <summary>
/// Sosyal medya platformları
/// </summary>
public enum SocialPlatform
{
    YouTube,
    TikTok,
    Instagram,
    Facebook,
    Twitter,
    LinkedIn
}

/// <summary>
/// Platform yapılandırması
/// </summary>
public class PlatformConfiguration
{
    public SocialPlatform Platform { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int MaxCharacterLimit { get; set; }
    public long MaxMediaSize { get; set; } // Bytes olarak
    public string[] SupportedMediaTypes { get; set; } = [];
    public bool SupportsScheduling { get; set; }
    public bool SupportsHashtags { get; set; }
    public bool SupportsMentions { get; set; }
    public bool SupportsLinks { get; set; }
    public bool SupportsVideo { get; set; }
    public bool SupportsImage { get; set; }
    public bool SupportsStories { get; set; }
    public bool SupportsReels { get; set; }
    public string OAuthAuthorizationUrl { get; set; } = string.Empty;
    public string ApiBaseUrl { get; set; } = string.Empty;
}

/// <summary>
/// Bağlantılı sosyal medya hesabı
/// </summary>
public class ConnectedAccount
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public SocialPlatform Platform { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public string AuthToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime TokenExpiresAt { get; set; }
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Platform konfigürasyonları
/// </summary>
public static class PlatformRegistry
{
    public static readonly Dictionary<SocialPlatform, PlatformConfiguration> Configurations = new()
    {
        {
            SocialPlatform.YouTube,
            new PlatformConfiguration
            {
                Platform = SocialPlatform.YouTube,
                Name = "YouTube",
                Icon = "youtube-logo",
                Color = "#FF0000",
                MaxCharacterLimit = 5000,
                MaxMediaSize = 5_000_000_000, // 5GB
                SupportedMediaTypes = ["video/mp4", "video/quicktime"],
                SupportsScheduling = true,
                SupportsHashtags = true,
                SupportsMentions = false,
                SupportsLinks = true,
                SupportsVideo = true,
                SupportsImage = false,
                SupportsStories = false,
                SupportsReels = false,
                OAuthAuthorizationUrl = "https://accounts.google.com/o/oauth2/v2/auth",
                ApiBaseUrl = "https://www.googleapis.com/youtube/v3"
            }
        },
        {
            SocialPlatform.TikTok,
            new PlatformConfiguration
            {
                Platform = SocialPlatform.TikTok,
                Name = "TikTok",
                Icon = "tiktok-logo",
                Color = "#000000",
                MaxCharacterLimit = 2200,
                MaxMediaSize = 287_000_000, // 287MB
                SupportedMediaTypes = ["video/mp4"],
                SupportsScheduling = true,
                SupportsHashtags = true,
                SupportsMentions = true,
                SupportsLinks = true,
                SupportsVideo = true,
                SupportsImage = false,
                SupportsStories = false,
                SupportsReels = true,
                OAuthAuthorizationUrl = "https://open.tiktok.com/oauth/",
                ApiBaseUrl = "https://open.tiktok.com/v1"
            }
        },
        {
            SocialPlatform.Instagram,
            new PlatformConfiguration
            {
                Platform = SocialPlatform.Instagram,
                Name = "Instagram",
                Icon = "instagram-logo",
                Color = "#E1306C",
                MaxCharacterLimit = 2200,
                MaxMediaSize = 8_000_000, // 8MB
                SupportedMediaTypes = ["image/jpeg", "image/png", "video/mp4"],
                SupportsScheduling = true,
                SupportsHashtags = true,
                SupportsMentions = true,
                SupportsLinks = false,
                SupportsVideo = true,
                SupportsImage = true,
                SupportsStories = true,
                SupportsReels = true,
                OAuthAuthorizationUrl = "https://api.instagram.com/oauth/authorize",
                ApiBaseUrl = "https://graph.instagram.com"
            }
        },
        {
            SocialPlatform.Facebook,
            new PlatformConfiguration
            {
                Platform = SocialPlatform.Facebook,
                Name = "Facebook",
                Icon = "facebook-logo",
                Color = "#1877F2",
                MaxCharacterLimit = 63206,
                MaxMediaSize = 4_000_000, // 4MB
                SupportedMediaTypes = ["image/jpeg", "image/png", "video/mp4"],
                SupportsScheduling = true,
                SupportsHashtags = true,
                SupportsMentions = true,
                SupportsLinks = true,
                SupportsVideo = true,
                SupportsImage = true,
                SupportsStories = false,
                SupportsReels = false,
                OAuthAuthorizationUrl = "https://www.facebook.com/v18.0/dialog/oauth",
                ApiBaseUrl = "https://graph.facebook.com/v18.0"
            }
        },
        {
            SocialPlatform.Twitter,
            new PlatformConfiguration
            {
                Platform = SocialPlatform.Twitter,
                Name = "Twitter/X",
                Icon = "twitter-logo",
                Color = "#000000",
                MaxCharacterLimit = 280,
                MaxMediaSize = 5_242_880, // 5MB
                SupportedMediaTypes = ["image/jpeg", "image/png", "video/mp4", "video/quicktime"],
                SupportsScheduling = true,
                SupportsHashtags = true,
                SupportsMentions = true,
                SupportsLinks = true,
                SupportsVideo = true,
                SupportsImage = true,
                SupportsStories = false,
                SupportsReels = false,
                OAuthAuthorizationUrl = "https://twitter.com/i/oauth2/authorize",
                ApiBaseUrl = "https://api.twitter.com/2"
            }
        },
        {
            SocialPlatform.LinkedIn,
            new PlatformConfiguration
            {
                Platform = SocialPlatform.LinkedIn,
                Name = "LinkedIn",
                Icon = "linkedin-logo",
                Color = "#0A66C2",
                MaxCharacterLimit = 3000,
                MaxMediaSize = 10_000_000, // 10MB
                SupportedMediaTypes = ["image/jpeg", "image/png", "video/mp4"],
                SupportsScheduling = true,
                SupportsHashtags = false,
                SupportsMentions = true,
                SupportsLinks = true,
                SupportsVideo = true,
                SupportsImage = true,
                SupportsStories = false,
                SupportsReels = false,
                OAuthAuthorizationUrl = "https://www.linkedin.com/oauth/v2/authorization",
                ApiBaseUrl = "https://api.linkedin.com/v2"
            }
        }
    };

    public static PlatformConfiguration? GetPlatform(SocialPlatform platform)
    {
        return Configurations.TryGetValue(platform, out var config) ? config : null;
    }

    public static IEnumerable<PlatformConfiguration> GetAllPlatforms()
    {
        return Configurations.Values;
    }

    public static int GetMaxCharacterLimit(SocialPlatform platform)
    {
        return GetPlatform(platform)?.MaxCharacterLimit ?? 280;
    }
}
