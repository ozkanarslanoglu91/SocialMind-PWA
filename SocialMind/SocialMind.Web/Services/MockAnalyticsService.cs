using SocialMind.Shared.Models;
using SocialMind.Shared.Services;

namespace SocialMind.Web.Services;

/// <summary>
/// Mock Analytics servisi - test ve geliştirme için
/// </summary>
public class MockAnalyticsService : IAnalyticsService
{
    private readonly Random _random = new();

    public Task<PlatformAnalytics> GetPlatformAnalyticsAsync(SocialPlatform platform)
    {
        var analytics = new PlatformAnalytics
        {
            Platform = platform,
            TotalFollowers = _random.Next(1000, 50000),
            TotalEngagement = _random.Next(500, 10000),
            TotalImpressions = _random.Next(10000, 100000),
            TotalClicks = _random.Next(1000, 20000),
            TotalShares = _random.Next(100, 5000),
            EngagementRate = (decimal)(_random.NextDouble() * 10),
            LastUpdated = DateTime.UtcNow
        };

        return Task.FromResult(analytics);
    }

    public async Task<List<PlatformAnalytics>> GetAllPlatformAnalyticsAsync()
    {
        var platforms = Enum.GetValues<SocialPlatform>();
        var analytics = new List<PlatformAnalytics>();

        foreach (var platform in platforms)
        {
            analytics.Add(await GetPlatformAnalyticsAsync(platform));
        }

        return analytics;
    }

    public Task<PostAnalytics> GetPostAnalyticsAsync(string postId)
    {
        var analytics = new PostAnalytics
        {
            PostId = postId,
            Platform = SocialPlatform.Twitter,
            PlatformPostId = Guid.NewGuid().ToString(),
            Likes = _random.Next(50, 1000),
            Comments = _random.Next(10, 200),
            Shares = _random.Next(5, 100),
            Impressions = _random.Next(1000, 10000),
            Clicks = _random.Next(100, 2000),
            Saves = _random.Next(10, 500),
            EngagementRate = (decimal)(_random.NextDouble() * 15),
            AnalyzedAt = DateTime.UtcNow
        };

        return Task.FromResult(analytics);
    }

    public Task<List<PostAnalytics>> GetTopPostsAsync(int limit = 10)
    {
        var posts = new List<PostAnalytics>();

        for (int i = 0; i < limit; i++)
        {
            posts.Add(new PostAnalytics
            {
                PostId = $"post_{i}",
                Platform = (SocialPlatform)(i % 6),
                Likes = _random.Next(100, 5000),
                Comments = _random.Next(20, 500),
                Shares = _random.Next(10, 200),
                Impressions = _random.Next(5000, 50000),
                EngagementRate = (decimal)(_random.NextDouble() * 20),
                AnalyzedAt = DateTime.UtcNow.AddDays(-i)
            });
        }

        return Task.FromResult(posts.OrderByDescending(p => p.EngagementRate).ToList());
    }

    public Task<List<PostAnalytics>> GetUpcomingPostsAnalyticsAsync()
    {
        return Task.FromResult(new List<PostAnalytics>());
    }

    public Task<BestTimeAnalysis> GetBestTimeAsync(SocialPlatform platform)
    {
        var analysis = new BestTimeAnalysis
        {
            Platform = platform,
            BestDay = (DayOfWeek)_random.Next(0, 7),
            BestHour = _random.Next(9, 18),
            AverageEngagementAtTime = (decimal)(_random.NextDouble() * 500),
            HourlyData = new List<HourlyEngagement>()
        };

        for (int hour = 0; hour < 24; hour++)
        {
            analysis.HourlyData.Add(new HourlyEngagement
            {
                Hour = hour,
                AvgEngagement = (decimal)(_random.NextDouble() * 300),
                PostCount = _random.Next(0, 10),
                TopHashtags = new List<string> { "tech", "ai", "social" }
            });
        }

        return Task.FromResult(analysis);
    }

    public async Task<List<BestTimeAnalysis>> GetAllBestTimesAsync()
    {
        var platforms = Enum.GetValues<SocialPlatform>();
        var analyses = new List<BestTimeAnalysis>();

        foreach (var platform in platforms)
        {
            analyses.Add(await GetBestTimeAsync(platform));
        }

        return analyses;
    }

    public Task<HashtagAnalytics> GetHashtagAnalyticsAsync(string hashtag, SocialPlatform platform)
    {
        var analytics = new HashtagAnalytics
        {
            Hashtag = hashtag,
            Platform = platform,
            UsageCount = _random.Next(10, 100),
            TotalReach = _random.Next(1000, 50000),
            TotalEngagement = _random.Next(500, 10000),
            AverageEngagementRate = (decimal)(_random.NextDouble() * 10),
            LastUsed = DateTime.UtcNow.AddDays(-_random.Next(0, 30))
        };

        return Task.FromResult(analytics);
    }

    public Task<List<HashtagAnalytics>> GetTopHashtagsAsync(int limit = 10)
    {
        var hashtags = new List<HashtagAnalytics>();
        var commonHashtags = new[] { "ai", "tech", "socialmedia", "marketing", "business", "startup", "innovation", "digital", "technology", "entrepreneur" };

        for (int i = 0; i < Math.Min(limit, commonHashtags.Length); i++)
        {
            hashtags.Add(new HashtagAnalytics
            {
                Hashtag = commonHashtags[i],
                Platform = (SocialPlatform)(i % 6),
                UsageCount = _random.Next(50, 500),
                TotalReach = _random.Next(10000, 200000),
                TotalEngagement = _random.Next(5000, 50000),
                AverageEngagementRate = (decimal)(_random.NextDouble() * 15),
                LastUsed = DateTime.UtcNow.AddDays(-_random.Next(0, 7))
            });
        }

        return Task.FromResult(hashtags.OrderByDescending(h => h.TotalEngagement).ToList());
    }

    public async Task<AnalyticsReport> GenerateReportAsync(DateTime startDate, DateTime endDate)
    {
        var report = new AnalyticsReport
        {
            StartDate = startDate,
            EndDate = endDate,
            GeneratedAt = DateTime.UtcNow,
            PlatformAnalytics = await GetAllPlatformAnalyticsAsync(),
            TopPerformingPosts = await GetTopPostsAsync(),
            BestTimes = await GetAllBestTimesAsync(),
            TopHashtags = await GetTopHashtagsAsync(),
            Summary = new Dictionary<string, object>
            {
                { "TotalPosts", _random.Next(50, 200) },
                { "TotalEngagement", _random.Next(5000, 50000) },
                { "AvgEngagementRate", (decimal)(_random.NextDouble() * 10) },
                { "TotalReach", _random.Next(50000, 500000) }
            }
        };

        return report;
    }

    public Task<AnalyticsReport> ExportAnalyticsAsync(DateTime startDate, DateTime endDate)
    {
        return GenerateReportAsync(startDate, endDate);
    }
}
