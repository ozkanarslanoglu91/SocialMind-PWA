using SocialMind.Shared.Models;

namespace SocialMind.Web.Services;

/// <summary>
/// TikTok API service interface
/// </summary>
public interface ITikTokService
{
    /// <summary>
    /// Upload video to TikTok
    /// </summary>
    Task<ServiceResult<Post>> UploadVideoAsync(
        string accessToken,
        string videoFilePath,
        Post post);

    /// <summary>
    /// Get TikTok creator info
    /// </summary>
    Task<ServiceResult<TikTokCreator>> GetCreatorInfoAsync(string accessToken);

    /// <summary>
    /// Get video analytics
    /// </summary>
    Task<ServiceResult<TikTokAnalytics>> GetVideoAnalyticsAsync(
        string accessToken,
        string videoId);

    /// <summary>
    /// Schedule video for later
    /// </summary>
    Task<ServiceResult<string>> ScheduleVideoAsync(
        string accessToken,
        string videoFilePath,
        Post post,
        DateTime scheduleTime);
}

/// <summary>
/// TikTok creator information
/// </summary>
public record TikTokCreator(
    string Id,
    string Username,
    string DisplayName,
    string AvatarLargeUrl,
    int FollowerCount,
    int FollowingCount,
    int VideoCount,
    long LikeCount);

/// <summary>
/// TikTok video analytics
/// </summary>
public record TikTokAnalytics(
    string VideoId,
    int Views,
    int Likes,
    int Comments,
    int Shares,
    int Plays,
    int Completes);

/// <summary>
/// Generic result wrapper - shared across services
/// </summary>
public record ServiceResult<T>(bool Success, T? Data, string? ErrorMessage, string? ErrorCode)
{
    public static ServiceResult<T> Ok(T data) => new(true, data, null, null);
    public static ServiceResult<T> Fail(string message, string code) => new(false, default, message, code);
}
