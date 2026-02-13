using System.Net.Http.Headers;
using System.Text.Json;
using SocialMind.Shared.Models;
using Microsoft.Extensions.Logging;

namespace SocialMind.Web.Services.Instagram;

/// <summary>
/// Instagram Graph API service implementation
/// </summary>
public class InstagramService : IInstagramService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<InstagramService> _logger;
    private const string GraphApiBaseUrl = "https://graph.facebook.com/v18.0";

    public InstagramService(HttpClient httpClient, ILogger<InstagramService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(GraphApiBaseUrl);
    }

    public async Task<ServiceResult<Post>> PublishPostAsync(
        string accessToken,
        string instagramAccountId,
        Post post)
    {
        try
        {
            _logger.LogInformation("Publishing post to Instagram account {AccountId}", instagramAccountId);

            // Validate inputs
            if (string.IsNullOrWhiteSpace(accessToken))
                return ServiceResult<Post>.Fail("Access token is required", "INVALID_TOKEN");

            if (string.IsNullOrWhiteSpace(instagramAccountId))
                return ServiceResult<Post>.Fail("Instagram account ID is required", "INVALID_ACCOUNT");

            if (post.MediaUrls == null || !post.MediaUrls.Any())
                return ServiceResult<Post>.Fail("At least one media URL is required", "NO_MEDIA");

            // Step 1: Create media container
            var containerId = await CreateMediaContainerAsync(
                accessToken,
                instagramAccountId,
                post.MediaUrls.First(),
                post.Content);

            if (string.IsNullOrEmpty(containerId))
                return ServiceResult<Post>.Fail("Failed to create media container", "CONTAINER_FAILED");

            // Step 2: Publish media container
            var publishedMediaId = await PublishMediaContainerAsync(
                accessToken,
                instagramAccountId,
                containerId);

            if (string.IsNullOrEmpty(publishedMediaId))
                return ServiceResult<Post>.Fail("Failed to publish media", "PUBLISH_FAILED");

            // Update post with published ID
            post.PlatformPostId = publishedMediaId;
            post.Status = PostStatus.Published;
            post.PublishedDate = DateTime.UtcNow;

            _logger.LogInformation("Successfully published post {PostId} to Instagram as {MediaId}",
                post.Id, publishedMediaId);

            return ServiceResult<Post>.Ok(post);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while publishing to Instagram");
            return ServiceResult<Post>.Fail($"Network error: {ex.Message}", "NETWORK_ERROR");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while publishing to Instagram");
            return ServiceResult<Post>.Fail($"Unexpected error: {ex.Message}", "UNKNOWN_ERROR");
        }
    }

    public async Task<ServiceResult<InstagramAccount>> GetAccountInfoAsync(
        string accessToken,
        string instagramAccountId)
    {
        try
        {
            var fields = "id,username,name,profile_picture_url,followers_count,follows_count,media_count";
            var url = $"/{instagramAccountId}?fields={fields}&access_token={accessToken}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var accountData = JsonSerializer.Deserialize<InstagramAccountResponse>(json);

            if (accountData == null)
                return ServiceResult<InstagramAccount>.Fail("Failed to parse account data", "PARSE_ERROR");

            var account = new InstagramAccount
            {
                Id = accountData.id ?? string.Empty,
                Username = accountData.username ?? string.Empty,
                Name = accountData.name ?? string.Empty,
                ProfilePictureUrl = accountData.profile_picture_url ?? string.Empty,
                FollowersCount = accountData.followers_count,
                FollowsCount = accountData.follows_count,
                MediaCount = accountData.media_count
            };

            return ServiceResult<InstagramAccount>.Ok(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Instagram account info");
            return ServiceResult<InstagramAccount>.Fail(ex.Message, "ACCOUNT_INFO_ERROR");
        }
    }

    public async Task<ServiceResult<InstagramInsights>> GetMediaInsightsAsync(
        string accessToken,
        string mediaId)
    {
        try
        {
            var metrics = "impressions,reach,engagement,likes,comments,saves,shares";
            var url = $"/{mediaId}/insights?metric={metrics}&access_token={accessToken}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var insightsData = JsonSerializer.Deserialize<InstagramInsightsResponse>(json);

            if (insightsData?.data == null)
                return ServiceResult<InstagramInsights>.Fail("Failed to parse insights data", "PARSE_ERROR");

            var insights = new InstagramInsights
            {
                MediaId = mediaId,
                Impressions = GetMetricValue(insightsData.data, "impressions"),
                Reach = GetMetricValue(insightsData.data, "reach"),
                Engagement = GetMetricValue(insightsData.data, "engagement"),
                Likes = GetMetricValue(insightsData.data, "likes"),
                Comments = GetMetricValue(insightsData.data, "comments"),
                Saves = GetMetricValue(insightsData.data, "saves"),
                Shares = GetMetricValue(insightsData.data, "shares"),
                Timestamp = DateTime.UtcNow
            };

            return ServiceResult<InstagramInsights>.Ok(insights);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Instagram media insights");
            return ServiceResult<InstagramInsights>.Fail(ex.Message, "INSIGHTS_ERROR");
        }
    }

    public async Task<ServiceResult<bool>> ValidateAccessTokenAsync(string accessToken)
    {
        try
        {
            var url = $"/me?access_token={accessToken}";
            var response = await _httpClient.GetAsync(url);

            return ServiceResult<bool>.Ok(response.IsSuccessStatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating access token");
            return ServiceResult<bool>.Fail(ex.Message, "TOKEN_VALIDATION_ERROR");
        }
    }

    private async Task<string?> CreateMediaContainerAsync(
        string accessToken,
        string instagramAccountId,
        string mediaUrl,
        string caption)
    {
        try
        {
            var url = $"/{instagramAccountId}/media";
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "image_url", mediaUrl },
                { "caption", caption },
                { "access_token", accessToken }
            });

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<MediaContainerResponse>(json);

            return result?.id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating media container");
            return null;
        }
    }

    private async Task<string?> PublishMediaContainerAsync(
        string accessToken,
        string instagramAccountId,
        string containerId)
    {
        try
        {
            var url = $"/{instagramAccountId}/media_publish";
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "creation_id", containerId },
                { "access_token", accessToken }
            });

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<MediaPublishResponse>(json);

            return result?.id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing media container");
            return null;
        }
    }

    private static int GetMetricValue(List<MetricData> data, string metricName)
    {
        var metric = data.FirstOrDefault(m => m.name == metricName);
        return metric?.values?.FirstOrDefault()?.value ?? 0;
    }

    // Response DTOs
    private record InstagramAccountResponse(
        string? id,
        string? username,
        string? name,
        string? profile_picture_url,
        int followers_count,
        int follows_count,
        int media_count
    );

    private record InstagramInsightsResponse(List<MetricData>? data);
    private record MetricData(string? name, string? period, List<MetricValue>? values);
    private record MetricValue(int value, DateTime end_time);

    private record MediaContainerResponse(string? id);
    private record MediaPublishResponse(string? id);
}
