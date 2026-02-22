using SocialMind.Shared.Models;

namespace SocialMind.Shared.Services;

public interface IPostService
{
    Task<List<Post>> GetPostsAsync();
    Task<Post?> GetPostAsync(string id);
    Task<Post> CreatePostAsync(Post post);
    Task<Post> UpdatePostAsync(Post post);
    Task<bool> DeletePostAsync(string id);
    Task<List<Post>> GetScheduledPostsAsync();
}

public interface IPlatformService
{
    Task<List<SocialAccount>> GetAccountsAsync();
    Task<SocialAccount> ConnectAccountAsync(string platform, string token);
    Task<bool> DisconnectAccountAsync(string accountId);
    Task<bool> TestConnectionAsync(string accountId);
}

public interface IAnalyticsService
{
    Task<List<Analytics>> GetAnalyticsAsync(DateTime from, DateTime to);
    Task<Analytics> GetPlatformAnalyticsAsync(string platform, DateTime date);
    Task<Dictionary<string, int>> GetTopPostsAsync();
    Task<List<string>> GetTrendingHashtagsAsync();
}

public interface IAIService
{
    Task<AIGenerationResponse> GenerateContentAsync(AIGenerationRequest request);
    Task<List<string>> GenerateHashtagsAsync(string content);
    Task<string> OptimizeContentAsync(string content, string platform);
}

public interface IScheduleService
{
    Task<List<Post>> GetUpcomingPostsAsync(int days);
    Task<bool> SchedulePostAsync(string postId, DateTime scheduledTime);
    Task<List<Post>> GetOptimalPostTimesAsync();
}

public interface ICampaignService
{
    Task<List<Campaign>> GetCampaignsAsync();
    Task<Campaign> CreateCampaignAsync(Campaign campaign);
    Task<Campaign> UpdateCampaignAsync(Campaign campaign);
    Task<bool> DeleteCampaignAsync(string campaignId);
}

public interface ISettingsService
{
    Task<Dictionary<string, string>> GetSettingsAsync();
    Task<bool> UpdateSettingAsync(string key, string value);
    Task<string?> GetSettingAsync(string key);
}
