using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SocialMind.Web.Services.YouTube;

/// <summary>
/// YouTube Data API v3 service implementation
/// </summary>
public class YouTubeService : IYouTubeService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<YouTubeService> _logger;
    private const string YouTubeApiBaseUrl = "https://www.googleapis.com/youtube/v3";

    public YouTubeService(HttpClient httpClient, ILogger<YouTubeService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(YouTubeApiBaseUrl);
    }

    public async Task<ServiceResult<Post>> UploadVideoAsync(
        string accessToken,
        string videoFilePath,
        Post post)
    {
        try
        {
            _logger.LogInformation("Uploading video to YouTube: {FilePath}", videoFilePath);

            if (string.IsNullOrWhiteSpace(accessToken))
                return ServiceResult<Post>.Fail("Access token is required", "INVALID_TOKEN");

            if (string.IsNullOrWhiteSpace(videoFilePath) || !File.Exists(videoFilePath))
                return ServiceResult<Post>.Fail("Video file not found", "FILE_NOT_FOUND");

            if (string.IsNullOrWhiteSpace(post.Content))
                return ServiceResult<Post>.Fail("Video title/description is required", "NO_CONTENT");

            // Upload video to YouTube
            var uploadRequest = new
            {
                snippet = new
                {
                    title = post.Content,
                    description = post.Content,
                    tags = new[] { "automated", "socialmind" },
                    categoryId = "22" // People & Blogs
                },
                status = new
                {
                    privacyStatus = "unlisted"
                }
            };

            using var content = new MultipartFormDataContent();
            using var fileStream = File.OpenRead(videoFilePath);
            content.Add(new StreamContent(fileStream), "video", Path.GetFileName(videoFilePath));
            content.Add(new StringContent(JsonSerializer.Serialize(uploadRequest)), "metadata");

            var uploadUrl = $"{YouTubeApiBaseUrl}/videos?part=snippet,status&access_token={accessToken}";
            var response = await _httpClient.PostAsync(uploadUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                _logger.LogError("YouTube upload failed: {Error}", errorMessage);
                return ServiceResult<Post>.Fail($"YouTube upload failed: {response.StatusCode}", "UPLOAD_FAILED");
            }

            var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
            var videoId = json.GetProperty("id").GetString();

            post.PlatformPostId = videoId;
            post.Status = PostStatus.Published;
            post.PublishedDate = DateTime.UtcNow;

            _logger.LogInformation("Video uploaded successfully: {VideoId}", videoId);
            return ServiceResult<Post>.Ok(post);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading video to YouTube");
            return ServiceResult<Post>.Fail(ex.Message, "NETWORK_ERROR");
        }
    }

    public async Task<ServiceResult<YouTubeChannel>> GetChannelInfoAsync(string accessToken)
    {
        try
        {
            _logger.LogInformation("Fetching YouTube channel info");

            if (string.IsNullOrWhiteSpace(accessToken))
                return ServiceResult<YouTubeChannel>.Fail("Access token is required", "INVALID_TOKEN");

            var url = $"{YouTubeApiBaseUrl}/channels?part=snippet,statistics&mine=true&access_token={accessToken}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch channel info: {StatusCode}", response.StatusCode);
                return ServiceResult<YouTubeChannel>.Fail("Failed to fetch channel info", "FETCH_FAILED");
            }

            var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
            var items = json.GetProperty("items");

            if (items.GetArrayLength() == 0)
                return ServiceResult<YouTubeChannel>.Fail("No channel found", "NO_CHANNEL");

            var channel = items[0];
            var snippet = channel.GetProperty("snippet");
            var statistics = channel.GetProperty("statistics");

            var channelInfo = new YouTubeChannel(
                Id: channel.GetProperty("id").GetString() ?? "",
                Title: snippet.GetProperty("title").GetString() ?? "",
                Description: snippet.GetProperty("description").GetString() ?? "",
                ThumbnailUrl: snippet.GetProperty("thumbnails").GetProperty("default").GetProperty("url").GetString() ?? "",
                SubscriberCount: int.TryParse(statistics.GetProperty("subscriberCount").GetString(), out var subs) ? subs : 0,
                VideoCount: int.TryParse(statistics.GetProperty("videoCount").GetString(), out var vids) ? vids : 0,
                ViewCount: int.TryParse(statistics.GetProperty("viewCount").GetString(), out var views) ? views : 0
            );

            return ServiceResult<YouTubeChannel>.Ok(channelInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching YouTube channel info");
            return ServiceResult<YouTubeChannel>.Fail(ex.Message, "NETWORK_ERROR");
        }
    }

    public async Task<ServiceResult<YouTubeAnalytics>> GetVideoAnalyticsAsync(
        string accessToken,
        string videoId)
    {
        try
        {
            _logger.LogInformation("Fetching analytics for video: {VideoId}", videoId);

            if (string.IsNullOrWhiteSpace(accessToken))
                return ServiceResult<YouTubeAnalytics>.Fail("Access token is required", "INVALID_TOKEN");

            if (string.IsNullOrWhiteSpace(videoId))
                return ServiceResult<YouTubeAnalytics>.Fail("Video ID is required", "INVALID_VIDEO_ID");

            var url = $"{YouTubeApiBaseUrl}/videos?part=statistics&id={videoId}&access_token={accessToken}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return ServiceResult<YouTubeAnalytics>.Fail("Failed to fetch analytics", "FETCH_FAILED");

            var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
            var items = json.GetProperty("items");

            if (items.GetArrayLength() == 0)
                return ServiceResult<YouTubeAnalytics>.Fail("Video not found", "NOT_FOUND");

            var statistics = items[0].GetProperty("statistics");

            var analytics = new YouTubeAnalytics(
                VideoId: videoId,
                Views: int.TryParse(statistics.GetProperty("viewCount").GetString(), out var views) ? views : 0,
                Likes: int.TryParse(statistics.GetProperty("likeCount").GetString(), out var likes) ? likes : 0,
                Comments: int.TryParse(statistics.GetProperty("commentCount").GetString(), out var comments) ? comments : 0,
                Shares: 0, // YouTube doesn't expose share count via API
                AverageViewDuration: 0, // Would need YouTube Analytics API Reporting
                ClickThroughRate: 0 // Would need YouTube Analytics API Reporting
            );

            return ServiceResult<YouTubeAnalytics>.Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching video analytics");
            return ServiceResult<YouTubeAnalytics>.Fail(ex.Message, "NETWORK_ERROR");
        }
    }

    public async Task<ServiceResult<string>> CreatePlaylistAsync(
        string accessToken,
        string title,
        string description)
    {
        try
        {
            _logger.LogInformation("Creating playlist: {Title}", title);

            if (string.IsNullOrWhiteSpace(accessToken))
                return ServiceResult<string>.Fail("Access token is required", "INVALID_TOKEN");

            if (string.IsNullOrWhiteSpace(title))
                return ServiceResult<string>.Fail("Playlist title is required", "INVALID_TITLE");

            var playlistData = new
            {
                snippet = new
                {
                    title,
                    description
                },
                status = new
                {
                    privacyStatus = "private"
                }
            };

            var url = $"{YouTubeApiBaseUrl}/playlists?part=snippet,status&access_token={accessToken}";
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(playlistData),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(url, jsonContent);

            if (!response.IsSuccessStatusCode)
                return ServiceResult<string>.Fail("Failed to create playlist", "CREATE_FAILED");

            var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
            var playlistId = json.GetProperty("id").GetString() ?? "";

            _logger.LogInformation("Playlist created: {PlaylistId}", playlistId);
            return ServiceResult<string>.Ok(playlistId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating playlist");
            return ServiceResult<string>.Fail(ex.Message, "NETWORK_ERROR");
        }
    }
}
