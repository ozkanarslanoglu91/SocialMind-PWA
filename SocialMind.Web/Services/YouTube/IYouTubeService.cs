using SocialMind.Shared.Models;

namespace SocialMind.Web.Services.YouTube;

/// <summary>
/// YouTube Data API service interface
/// </summary>
public interface IYouTubeService
{
    /// <summary>
    /// Upload video to YouTube
    /// </summary>
    Task<ServiceResult<Post>> UploadVideoAsync(
        string accessToken,
        string videoFilePath,
        Post post);

    /// <summary>
    /// Get YouTube channel info
    /// </summary>
    Task<ServiceResult<YouTubeChannel>> GetChannelInfoAsync(string accessToken);

    /// <summary>
    /// Get video analytics
    /// </summary>
    Task<ServiceResult<YouTubeAnalytics>> GetVideoAnalyticsAsync(
        string accessToken,
        string videoId);

    /// <summary>
    /// Create playlist
    /// </summary>
    Task<ServiceResult<string>> CreatePlaylistAsync(
        string accessToken,
        string title,
        string description);
}

/// <summary>
/// YouTube channel information
/// </summary>
public record YouTubeChannel(
    string Id,
    string Title,
    string Description,
    string ThumbnailUrl,
    int SubscriberCount,
    int VideoCount,
    int ViewCount);

/// <summary>
/// YouTube video analytics
/// </summary>
public record YouTubeAnalytics(
    string VideoId,
    int Views,
    int Likes,
    int Comments,
    int Shares,
    double AverageViewDuration,
    double ClickThroughRate);

/// <summary>
/// Generic result wrapper for service operations
/// </summary>
public record ServiceResult<T>(bool Success, T? Data, string? ErrorMessage, string? ErrorCode)
{
    public static ServiceResult<T> Ok(T data) => new(true, data, null, null);
    public static ServiceResult<T> Fail(string message, string code) => new(false, default, message, code);
}
