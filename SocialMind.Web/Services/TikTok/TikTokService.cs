using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SocialMind.Shared.Models;

namespace SocialMind.Web.Services.TikTok;

/// <summary>
/// TikTok Graph API v1 service implementation
/// </summary>
public class TikTokService : ITikTokService
{
    private const string TikTokApiBaseUrl = "https://open.tiktok.com/v1";
    private const int TimeoutSeconds = 120;

    private readonly HttpClient _httpClient;
    private readonly ILogger<TikTokService> _logger;

    public TikTokService(HttpClient httpClient, ILogger<TikTokService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.Timeout = TimeSpan.FromSeconds(TimeoutSeconds);
    }

    /// <summary>
    /// Upload video to TikTok with chunked file transfer
    /// </summary>
    public async Task<ServiceResult<Post>> UploadVideoAsync(
        string accessToken,
        string videoFilePath,
        Post post)
    {
        try
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("TikTok: Access token is empty");
                return ServiceResult<Post>.Fail("Invalid access token", "INVALID_TOKEN");
            }

            if (!File.Exists(videoFilePath))
            {
                _logger.LogWarning("TikTok: Video file not found: {FilePath}", videoFilePath);
                return ServiceResult<Post>.Fail($"Video file not found: {videoFilePath}", "FILE_NOT_FOUND");
            }

            // Step 1: Initialize upload
            var fileInfo = new FileInfo(videoFilePath);
            var uploadInitRequest = new
            {
                source_info = new
                {
                    source = "FILE_UPLOAD",
                    file_name = Path.GetFileName(videoFilePath),
                    file_size = fileInfo.Length,
                    chunk_size = 5242880 // 5MB chunks
                }
            };

            var initUrl = $"{TikTokApiBaseUrl}/video/upload/init/?access_token={accessToken}";
            var initContent = new StringContent(
                JsonSerializer.Serialize(uploadInitRequest),
                Encoding.UTF8,
                "application/json");

            _logger.LogInformation("TikTok: Initializing video upload for {FileName}", Path.GetFileName(videoFilePath));

            var initResponse = await _httpClient.PostAsync(initUrl, initContent);
            if (!initResponse.IsSuccessStatusCode)
            {
                var errorContent = await initResponse.Content.ReadAsStringAsync();
                _logger.LogError("TikTok: Upload initialization failed: {StatusCode} {Error}", initResponse.StatusCode, errorContent);
                return ServiceResult<Post>.Fail("Failed to initialize video upload", "UPLOAD_INIT_FAILED");
            }

            var initJson = JsonSerializer.Deserialize<JsonElement>(await initResponse.Content.ReadAsStringAsync());
            var uploadId = initJson.GetProperty("data").GetProperty("upload_id").GetString();

            // Step 2: Upload video in chunks
            _logger.LogInformation("TikTok: Uploading video chunks for uploadId: {UploadId}", uploadId);

            const int chunkSize = 5242880; // 5MB
            using var fileStream = File.OpenRead(videoFilePath);
            int partNumber = 1;
            byte[] buffer = new byte[chunkSize];
            int bytesRead;

            while ((bytesRead = await fileStream.ReadAsync(buffer, 0, chunkSize)) > 0)
            {
                var uploadChunkRequest = new
                {
                    upload_id = uploadId,
                    chunk_num = partNumber,
                    total_chunk_num = (int)Math.Ceiling((double)fileInfo.Length / chunkSize)
                };

                using var chunkContent = new MultipartFormDataContent();
                chunkContent.Add(new ByteArrayContent(buffer, 0, bytesRead), "video");

                var chunkUrl = $"{TikTokApiBaseUrl}/video/upload/?access_token={accessToken}&upload_id={uploadId}&chunk_num={partNumber}&total_chunk_num={(int)Math.Ceiling((double)fileInfo.Length / chunkSize)}";
                var chunkResponse = await _httpClient.PostAsync(chunkUrl, chunkContent);

                if (!chunkResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("TikTok: Chunk upload failed at part {PartNumber}: {StatusCode}", partNumber, chunkResponse.StatusCode);
                    return ServiceResult<Post>.Fail($"Failed to upload chunk {partNumber}", "CHUNK_UPLOAD_FAILED");
                }

                _logger.LogInformation("TikTok: Successfully uploaded chunk {PartNumber}/{TotalChunks}", partNumber, (int)Math.Ceiling((double)fileInfo.Length / chunkSize));
                partNumber++;
            }

            // Step 3: Complete upload and publish
            var publishRequest = new
            {
                upload_id = uploadId,
                video_title = post.Caption ?? "Posted via SocialMind",
                text_extra = new[] { new { start = 0, end = 1, type = 1, user_id = "" } }, // Placeholder for mentions
                disable_comment = false,
                disable_duet = false,
                disable_stitch = false
            };

            var publishUrl = $"{TikTokApiBaseUrl}/video/publish/?access_token={accessToken}";
            var publishContent = new StringContent(
                JsonSerializer.Serialize(publishRequest),
                Encoding.UTF8,
                "application/json");

            _logger.LogInformation("TikTok: Publishing video with uploadId: {UploadId}", uploadId);

            var publishResponse = await _httpClient.PostAsync(publishUrl, publishContent);
            if (!publishResponse.IsSuccessStatusCode)
            {
                var errorContent = await publishResponse.Content.ReadAsStringAsync();
                _logger.LogError("TikTok: Video publish failed: {StatusCode} {Error}", publishResponse.StatusCode, errorContent);
                return ServiceResult<Post>.Fail("Failed to publish video", "PUBLISH_FAILED");
            }

            var publishJson = JsonSerializer.Deserialize<JsonElement>(await publishResponse.Content.ReadAsStringAsync());
            var videoId = publishJson.GetProperty("data").GetProperty("video_id").GetString();

            var resultPost = post with { ExternalId = videoId, PublishedAt = DateTime.UtcNow };

            _logger.LogInformation("TikTok: Video published successfully with ID: {VideoId}", videoId);
            return ServiceResult<Post>.Ok(resultPost);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "TikTok: Network error during video upload");
            return ServiceResult<Post>.Fail($"Network error: {ex.Message}", "NETWORK_ERROR");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TikTok: Unexpected error during video upload");
            return ServiceResult<Post>.Fail($"Unexpected error: {ex.Message}", "UNKNOWN_ERROR");
        }
    }

    /// <summary>
    /// Get creator information from TikTok
    /// </summary>
    public async Task<ServiceResult<TikTokCreator>> GetCreatorInfoAsync(string accessToken)
    {
        try
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("TikTok: Access token is empty");
                return ServiceResult<TikTokCreator>.Fail("Invalid access token", "INVALID_TOKEN");
            }

            var url = $"{TikTokApiBaseUrl}/user/info/?access_token={accessToken}&fields=open_id,union_id,user_id,display_name,avatar_large_url,follower_count,following_count,video_count,like_count";

            _logger.LogInformation("TikTok: Fetching creator info");

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("TikTok: Failed to fetch creator info: {StatusCode} {Error}", response.StatusCode, errorContent);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return ServiceResult<TikTokCreator>.Fail("Invalid or expired access token", "INVALID_TOKEN");

                return ServiceResult<TikTokCreator>.Fail("Failed to fetch creator info", "FETCH_FAILED");
            }

            var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
            var userData = json.GetProperty("data").GetProperty("user");

            var creator = new TikTokCreator(
                Id: userData.GetProperty("open_id").GetString() ?? "",
                Username: userData.GetProperty("display_name").GetString() ?? "Unknown",
                DisplayName: userData.GetProperty("display_name").GetString() ?? "Unknown",
                AvatarLargeUrl: userData.GetProperty("avatar_large_url").GetString() ?? "",
                FollowerCount: userData.GetProperty("follower_count").GetInt32(),
                FollowingCount: userData.GetProperty("following_count").GetInt32(),
                VideoCount: userData.GetProperty("video_count").GetInt32(),
                LikeCount: userData.GetProperty("like_count").GetInt64());

            _logger.LogInformation("TikTok: Retrieved creator info for {Username}", creator.Username);
            return ServiceResult<TikTokCreator>.Ok(creator);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "TikTok: Network error fetching creator info");
            return ServiceResult<TikTokCreator>.Fail($"Network error: {ex.Message}", "NETWORK_ERROR");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TikTok: Unexpected error fetching creator info");
            return ServiceResult<TikTokCreator>.Fail($"Unexpected error: {ex.Message}", "UNKNOWN_ERROR");
        }
    }

    /// <summary>
    /// Get video analytics from TikTok
    /// </summary>
    public async Task<ServiceResult<TikTokAnalytics>> GetVideoAnalyticsAsync(
        string accessToken,
        string videoId)
    {
        try
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("TikTok: Access token is empty");
                return ServiceResult<TikTokAnalytics>.Fail("Invalid access token", "INVALID_TOKEN");
            }

            if (string.IsNullOrEmpty(videoId))
            {
                _logger.LogWarning("TikTok: Video ID is empty");
                return ServiceResult<TikTokAnalytics>.Fail("Invalid video ID", "INVALID_VIDEO_ID");
            }

            var url = $"{TikTokApiBaseUrl}/video/query/?access_token={accessToken}&fields=id,create_time,like_count,comment_count,share_count,play_count,reach";

            var queryRequest = new { filters = new { video_ids = new[] { videoId } } };
            var content = new StringContent(
                JsonSerializer.Serialize(queryRequest),
                Encoding.UTF8,
                "application/json");

            _logger.LogInformation("TikTok: Fetching analytics for video {VideoId}", videoId);

            var response = await _httpClient.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("TikTok: Failed to fetch video analytics: {StatusCode} {Error}", response.StatusCode, errorContent);
                return ServiceResult<TikTokAnalytics>.Fail("Failed to fetch video analytics", "FETCH_FAILED");
            }

            var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
            var videoData = json.GetProperty("data").GetProperty("videos")[0];

            var analytics = new TikTokAnalytics(
                VideoId: videoData.GetProperty("id").GetString() ?? videoId,
                Views: videoData.GetProperty("play_count").GetInt32(),
                Likes: videoData.GetProperty("like_count").GetInt32(),
                Comments: videoData.GetProperty("comment_count").GetInt32(),
                Shares: videoData.GetProperty("share_count").GetInt32(),
                Plays: videoData.GetProperty("play_count").GetInt32(),
                Completes: 0); // Not available in TikTok API

            _logger.LogInformation("TikTok: Retrieved analytics for video {VideoId}: {Views} views, {Likes} likes", videoId, analytics.Views, analytics.Likes);
            return ServiceResult<TikTokAnalytics>.Ok(analytics);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "TikTok: Network error fetching video analytics");
            return ServiceResult<TikTokAnalytics>.Fail($"Network error: {ex.Message}", "NETWORK_ERROR");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TikTok: Unexpected error fetching video analytics");
            return ServiceResult<TikTokAnalytics>.Fail($"Unexpected error: {ex.Message}", "UNKNOWN_ERROR");
        }
    }

    /// <summary>
    /// Schedule video for later publishing
    /// </summary>
    public async Task<ServiceResult<string>> ScheduleVideoAsync(
        string accessToken,
        string videoFilePath,
        Post post,
        DateTime scheduleTime)
    {
        try
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("TikTok: Access token is empty");
                return ServiceResult<string>.Fail("Invalid access token", "INVALID_TOKEN");
            }

            if (!File.Exists(videoFilePath))
            {
                _logger.LogWarning("TikTok: Video file not found: {FilePath}", videoFilePath);
                return ServiceResult<string>.Fail($"Video file not found: {videoFilePath}", "FILE_NOT_FOUND");
            }

            if (scheduleTime <= DateTime.UtcNow)
            {
                _logger.LogWarning("TikTok: Schedule time must be in the future");
                return ServiceResult<string>.Fail("Schedule time must be in the future", "INVALID_SCHEDULE_TIME");
            }

            // Step 1: Initialize upload
            var fileInfo = new FileInfo(videoFilePath);
            var uploadInitRequest = new
            {
                source_info = new
                {
                    source = "FILE_UPLOAD",
                    file_name = Path.GetFileName(videoFilePath),
                    file_size = fileInfo.Length,
                    chunk_size = 5242880
                }
            };

            var initUrl = $"{TikTokApiBaseUrl}/video/upload/init/?access_token={accessToken}";
            var initContent = new StringContent(
                JsonSerializer.Serialize(uploadInitRequest),
                Encoding.UTF8,
                "application/json");

            _logger.LogInformation("TikTok: Initializing scheduled video upload");

            var initResponse = await _httpClient.PostAsync(initUrl, initContent);
            if (!initResponse.IsSuccessStatusCode)
            {
                _logger.LogError("TikTok: Scheduled upload initialization failed");
                return ServiceResult<string>.Fail("Failed to initialize video upload", "UPLOAD_INIT_FAILED");
            }

            var initJson = JsonSerializer.Deserialize<JsonElement>(await initResponse.Content.ReadAsStringAsync());
            var uploadId = initJson.GetProperty("data").GetProperty("upload_id").GetString();

            // Step 2: Upload video chunks (same as regular upload)
            const int chunkSize = 5242880;
            using var fileStream = File.OpenRead(videoFilePath);
            int partNumber = 1;
            byte[] buffer = new byte[chunkSize];
            int bytesRead;

            while ((bytesRead = await fileStream.ReadAsync(buffer, 0, chunkSize)) > 0)
            {
                using var chunkContent = new MultipartFormDataContent();
                chunkContent.Add(new ByteArrayContent(buffer, 0, bytesRead), "video");

                var chunkUrl = $"{TikTokApiBaseUrl}/video/upload/?access_token={accessToken}&upload_id={uploadId}&chunk_num={partNumber}&total_chunk_num={(int)Math.Ceiling((double)fileInfo.Length / chunkSize)}";
                var chunkResponse = await _httpClient.PostAsync(chunkUrl, chunkContent);

                if (!chunkResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("TikTok: Scheduled chunk upload failed at part {PartNumber}", partNumber);
                    return ServiceResult<string>.Fail($"Failed to upload chunk {partNumber}", "CHUNK_UPLOAD_FAILED");
                }

                partNumber++;
            }

            // Step 3: Schedule publish
            var unixScheduleTime = new DateTimeOffset(scheduleTime).ToUnixTimeSeconds();
            var scheduleRequest = new
            {
                upload_id = uploadId,
                video_title = post.Caption ?? "Posted via SocialMind",
                publish_type = "SCHEDULED_PUBLISH",
                publish_time = unixScheduleTime
            };

            var scheduleUrl = $"{TikTokApiBaseUrl}/video/publish/?access_token={accessToken}";
            var scheduleContent = new StringContent(
                JsonSerializer.Serialize(scheduleRequest),
                Encoding.UTF8,
                "application/json");

            _logger.LogInformation("TikTok: Scheduling video for {ScheduleTime}", scheduleTime);

            var scheduleResponse = await _httpClient.PostAsync(scheduleUrl, scheduleContent);
            if (!scheduleResponse.IsSuccessStatusCode)
            {
                var errorContent = await scheduleResponse.Content.ReadAsStringAsync();
                _logger.LogError("TikTok: Video scheduling failed: {StatusCode} {Error}", scheduleResponse.StatusCode, errorContent);
                return ServiceResult<string>.Fail("Failed to schedule video", "SCHEDULE_FAILED");
            }

            var publishJson = JsonSerializer.Deserialize<JsonElement>(await scheduleResponse.Content.ReadAsStringAsync());
            var videoId = publishJson.GetProperty("data").GetProperty("video_id").GetString();

            _logger.LogInformation("TikTok: Video scheduled successfully with ID: {VideoId} for {ScheduleTime}", videoId, scheduleTime);
            return ServiceResult<string>.Ok(videoId);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "TikTok: Network error during video scheduling");
            return ServiceResult<string>.Fail($"Network error: {ex.Message}", "NETWORK_ERROR");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TikTok: Unexpected error during video scheduling");
            return ServiceResult<string>.Fail($"Unexpected error: {ex.Message}", "UNKNOWN_ERROR");
        }
    }
}
