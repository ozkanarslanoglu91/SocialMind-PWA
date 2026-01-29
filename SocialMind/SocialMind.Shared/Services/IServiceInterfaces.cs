using SocialMind.Shared.Models;

namespace SocialMind.Shared.Services;

/// <summary>
/// AI hizmetleri için interface
/// </summary>
public interface IAIService
{
    Task<AIGeneratedContent> GenerateContentAsync(string prompt, string modelId, string language = "tr");
    Task<List<AIGeneratedContent>> GenerateMultipleContentAsync(string prompt, string modelId, int count = 3);
    Task<string> TranslateContentAsync(string content, string targetLanguage, string modelId);
    Task<string> OptimizeContentAsync(string content, SocialPlatform platform, string modelId);
    Task<List<string>> GenerateHashtagsAsync(string content, SocialPlatform platform, int count = 10);
    Task<string> AnalyzeSentimentAsync(string content, string modelId);
    Task<bool> IsValidApiKeyAsync(string apiKey);
}

/// <summary>
/// AI model factory
/// </summary>
public interface IAIModelFactory
{
    IAIService CreateService(AIProvider provider, string apiKey);
    Task<bool> ValidateProviderAsync(AIProvider provider, string apiKey);
}

/// <summary>
/// Post yönetim servisi
/// </summary>
public interface IPostService
{
    // CRUD operasyonları
    Task<Post> CreatePostAsync(Post post);
    Task<Post> UpdatePostAsync(Post post);
    Task<bool> DeletePostAsync(string postId);
    Task<Post?> GetPostAsync(string postId);
    Task<List<Post>> GetAllPostsAsync(PostStatus? status = null);
    Task<List<Post>> GetPostsByPlatformAsync(SocialPlatform platform);
    Task<List<Post>> GetScheduledPostsAsync();

    // Media operasyonları
    Task<PostMedia> AddMediaAsync(string postId, PostMedia media);
    Task<bool> RemoveMediaAsync(string postId, string mediaId);
    Task<List<PostMedia>> GetPostMediaAsync(string postId);

    // Yayın operasyonları
    Task<PublishedPostResult> PublishPostAsync(string postId);
    Task<List<PublishedPostResult>> PublishToMultiplePlatformsAsync(string postId);
    Task<bool> ReschedulePostAsync(string postId, DateTime newTime);
}

/// <summary>
/// Platform hesap yönetimi servisi
/// </summary>
public interface IPlatformService
{
    // OAuth operasyonları
    Task<ConnectedAccount> ConnectAccountAsync(SocialPlatform platform, string authCode);
    Task<bool> DisconnectAccountAsync(string accountId);
    Task<bool> RefreshTokenAsync(string accountId);
    Task<ConnectedAccount?> GetAccountAsync(string accountId);
    Task<List<ConnectedAccount>> GetConnectedAccountsAsync(SocialPlatform? platform = null);
    Task<List<ConnectedAccount>> GetAllConnectedAccountsAsync();

    // Platform bilgileri
    Task<PlatformConfiguration> GetPlatformConfigAsync(SocialPlatform platform);
    Task<string> GetPlatformAuthUrlAsync(SocialPlatform platform);

    // Test operasyonları
    Task<bool> TestConnectionAsync(string accountId);
}

/// <summary>
/// Analitik servisi
/// </summary>
public interface IAnalyticsService
{
    // Platform analitikleri
    Task<PlatformAnalytics> GetPlatformAnalyticsAsync(SocialPlatform platform);
    Task<List<PlatformAnalytics>> GetAllPlatformAnalyticsAsync();
    
    // Post analitikleri
    Task<PostAnalytics> GetPostAnalyticsAsync(string postId);
    Task<List<PostAnalytics>> GetTopPostsAsync(int limit = 10);
    Task<List<PostAnalytics>> GetUpcomingPostsAnalyticsAsync();

    // En iyi zamanlar
    Task<BestTimeAnalysis> GetBestTimeAsync(SocialPlatform platform);
    Task<List<BestTimeAnalysis>> GetAllBestTimesAsync();

    // Hashtag analizi
    Task<HashtagAnalytics> GetHashtagAnalyticsAsync(string hashtag, SocialPlatform platform);
    Task<List<HashtagAnalytics>> GetTopHashtagsAsync(int limit = 10);

    // Rapor oluşturma
    Task<AnalyticsReport> GenerateReportAsync(DateTime startDate, DateTime endDate);
    Task<AnalyticsReport> ExportAnalyticsAsync(DateTime startDate, DateTime endDate);
}

/// <summary>
/// Zamanlama servisi
/// </summary>
public interface IScheduleService
{
    // Zamanlama yönetimi
    Task<ScheduleConfiguration> CreateScheduleAsync(ScheduleConfiguration schedule);
    Task<ScheduleConfiguration> UpdateScheduleAsync(ScheduleConfiguration schedule);
    Task<bool> DeleteScheduleAsync(string scheduleId);
    Task<ScheduleConfiguration?> GetScheduleAsync(string scheduleId);
    Task<List<ScheduleConfiguration>> GetSchedulesForPostAsync(string postId);
    Task<List<ScheduleConfiguration>> GetAllSchedulesAsync();

    // Akıllı zamanlama
    Task<SmartScheduleSuggestion> GetOptimalScheduleSuggestionAsync(string postId, SocialPlatform platform);
    Task<List<SmartScheduleSuggestion>> GetOptimalScheduleSuggestionsAsync(string postId);

    // Takvim operasyonları
    Task<PostCalendar> GetCalendarForMonthAsync(int year, int month);
    Task<List<PostCalendarItem>> GetUpcomingPostsAsync(int daysAhead = 7);
    Task<List<PostCalendarItem>> GetPostsForDateAsync(DateTime date);

    // İş planlaması
    Task<bool> ExecuteScheduledPostsAsync();
}

/// <summary>
/// Kampanya yönetim servisi
/// </summary>
public interface ICampaignService
{
    // Kampanya operasyonları
    Task<CampaignAnalytics> CreateCampaignAsync(CampaignAnalytics campaign);
    Task<CampaignAnalytics> UpdateCampaignAsync(CampaignAnalytics campaign);
    Task<bool> DeleteCampaignAsync(string campaignId);
    Task<CampaignAnalytics?> GetCampaignAsync(string campaignId);
    Task<List<CampaignAnalytics>> GetAllCampaignsAsync();
    Task<List<CampaignAnalytics>> GetActiveCampaignsAsync();

    // Kampanya analitikleri
    Task<CampaignAnalytics> GetCampaignPerformanceAsync(string campaignId);
    Task<List<CampaignAnalytics>> GetCampaignsByPlatformAsync(SocialPlatform platform);
}

/// <summary>
/// Ayarlar servisi
/// </summary>
public interface ISettingsService
{
    // API anahtar yönetimi
    Task<bool> SaveApiKeyAsync(AIProvider provider, string apiKey);
    Task<string?> GetApiKeyAsync(AIProvider provider);
    Task<bool> ValidateApiKeyAsync(AIProvider provider, string apiKey);
    Task<bool> DeleteApiKeyAsync(AIProvider provider);

    // Tercihler
    Task<Dictionary<string, object>> GetUserPreferencesAsync();
    Task<bool> SaveUserPreferencesAsync(Dictionary<string, object> preferences);
    Task<T?> GetPreferenceAsync<T>(string key);
    Task<bool> SavePreferenceAsync<T>(string key, T value);
}
