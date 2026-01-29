namespace SocialMind.Shared.Models;

/// <summary>
/// Platform istatistikleri
/// </summary>
public class PlatformAnalytics
{
    public SocialPlatform Platform { get; set; }
    public int TotalFollowers { get; set; }
    public int TotalEngagement { get; set; }
    public int TotalImpressions { get; set; }
    public int TotalClicks { get; set; }
    public int TotalShares { get; set; }
    public decimal EngagementRate { get; set; }
    public List<PostAnalytics> TopPosts { get; set; } = [];
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Gönderi analitikleri
/// </summary>
public class PostAnalytics
{
    public string PostId { get; set; } = string.Empty;
    public SocialPlatform Platform { get; set; }
    public string PlatformPostId { get; set; } = string.Empty;
    public int Likes { get; set; }
    public int Comments { get; set; }
    public int Shares { get; set; }
    public int Impressions { get; set; }
    public int Clicks { get; set; }
    public int Saves { get; set; }
    public decimal EngagementRate { get; set; }
    public DateTime? AnalyzedAt { get; set; }
    public Dictionary<string, int>? DemographicData { get; set; }
    public List<CommentAnalytics>? TopComments { get; set; }
}

/// <summary>
/// Yorum analitikleri
/// </summary>
public class CommentAnalytics
{
    public string CommentId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string CommentText { get; set; } = string.Empty;
    public int Likes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Sentiment { get; set; } = "neutral"; // positive, negative, neutral
}

/// <summary>
/// Kampanya performans analitikleri
/// </summary>
public class CampaignAnalytics
{
    public string CampaignId { get; set; } = string.Empty;
    public string CampaignName { get; set; } = string.Empty;
    public decimal BudgetSpent { get; set; }
    public int Impressions { get; set; }
    public int Clicks { get; set; }
    public decimal CPC { get; set; } // Cost Per Click
    public decimal CPM { get; set; } // Cost Per Mille (1000 impressions)
    public decimal ROI { get; set; }
    public List<PlatformCampaignData> PlatformData { get; set; } = [];
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Platform kampanya verisi
/// </summary>
public class PlatformCampaignData
{
    public SocialPlatform Platform { get; set; }
    public decimal BudgetAllocated { get; set; }
    public int Impressions { get; set; }
    public int Clicks { get; set; }
    public int Conversions { get; set; }
    public decimal ROAS { get; set; } // Return on Ad Spend
}

/// <summary>
/// En iyi performans zamanları önerileri
/// </summary>
public class BestTimeAnalysis
{
    public SocialPlatform Platform { get; set; }
    public DayOfWeek BestDay { get; set; }
    public int BestHour { get; set; } // 0-23
    public decimal AverageEngagementAtTime { get; set; }
    public List<HourlyEngagement> HourlyData { get; set; } = [];
}

/// <summary>
/// Saatlik katılım verisi
/// </summary>
public class HourlyEngagement
{
    public int Hour { get; set; }
    public decimal AvgEngagement { get; set; }
    public int PostCount { get; set; }
    public List<string> TopHashtags { get; set; } = [];
}

/// <summary>
/// Hashtag performans analizi
/// </summary>
public class HashtagAnalytics
{
    public string Hashtag { get; set; } = string.Empty;
    public SocialPlatform Platform { get; set; }
    public int UsageCount { get; set; }
    public int TotalReach { get; set; }
    public int TotalEngagement { get; set; }
    public decimal AverageEngagementRate { get; set; }
    public DateTime LastUsed { get; set; }
}

/// <summary>
/// Analitik raporu
/// </summary>
public class AnalyticsReport
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<PlatformAnalytics> PlatformAnalytics { get; set; } = [];
    public List<PostAnalytics> TopPerformingPosts { get; set; } = [];
    public List<BestTimeAnalysis> BestTimes { get; set; } = [];
    public List<HashtagAnalytics> TopHashtags { get; set; } = [];
    public Dictionary<string, object>? Summary { get; set; }
}
