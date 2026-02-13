Chrome DevTools MCP
using SocialMind.Shared.Models;

namespace SocialMind.Web.Services.Instagram;

/// <summary>
/// Instagram Graph API service interface
/// </summary>
public interface IInstagramService
{
    /// <summary>
    /// Publish a post to Instagram
    /// </summary>
    Task<ServiceResult<Post>> PublishPostAsync(
        string accessToken,
        string instagramAccountId,
        Post post);

    /// <summary>
    /// Get Instagram account info
    /// </summary>
    Task<ServiceResult<InstagramAccount>> GetAccountInfoAsync(
        string accessToken,
        string instagramAccountId);

    /// <summary>
    /// Get Instagram media insights (analytics)
    /// </summary>
    Task<ServiceResult<InstagramInsights>> GetMediaInsightsAsync(
        string accessToken,
        string mediaId);

    /// <summary>
    /// Validate Instagram access token
    /// </summary>
    Task<ServiceResult<bool>> ValidateAccessTokenAsync(string accessToken);
}

/// <summary>
/// Instagram account information
/// </summary>
public class InstagramAccount
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ProfilePictureUrl { get; set; } = string.Empty;
    public int FollowersCount { get; set; }
    public int FollowsCount { get; set; }
    public int MediaCount { get; set; }
}

/// <summary>
/// Instagram media insights
/// </summary>
public class InstagramInsights
{
    public string MediaId { get; set; } = string.Empty;
    public int Impressions { get; set; }
    public int Reach { get; set; }
    public int Engagement { get; set; }
    public int Likes { get; set; }
    public int Comments { get; set; }
    public int Saves { get; set; }
    public int Shares { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Generic service result wrapper
/// </summary>
public class ServiceResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }

    public static ServiceResult<T> Ok(T data) => new()
    {
        Success = true,
        Data = data
    };

    public static ServiceResult<T> Fail(string errorMessage, string? errorCode = null) => new()
    {
        Success = false,
        ErrorMessage = errorMessage,
        ErrorCode = errorCode
    };
}
