using SocialMind.Shared.Models;

namespace SocialMind.Shared.Services;

public class MockPostService : IPostService
{
    private List<Post> _posts = new();

    public MockPostService()
    {
        InitializeMockData();
    }

    private void InitializeMockData()
    {
        _posts = new()
        {
            new Post
            {
                Content = "🚀 Excited to announce SocialMind - your AI-powered social media manager!",
                Platforms = new() { "Twitter", "LinkedIn" },
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Post
            {
                Content = "Just launched our new analytics dashboard. Check out the performance metrics! 📊",
                Platforms = new() { "Instagram", "Facebook" },
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };
    }

    public Task<List<Post>> GetPostsAsync() => Task.FromResult(_posts);

    public Task<Post?> GetPostAsync(string id) => Task.FromResult(_posts.FirstOrDefault(p => p.Id == id));

    public Task<Post> CreatePostAsync(Post post)
    {
        _posts.Add(post);
        return Task.FromResult(post);
    }

    public Task<Post> UpdatePostAsync(Post post)
    {
        var index = _posts.FindIndex(p => p.Id == post.Id);
        if (index >= 0)
            _posts[index] = post;
        return Task.FromResult(post);
    }

    public Task<bool> DeletePostAsync(string id)
    {
        var post = _posts.FirstOrDefault(p => p.Id == id);
        if (post != null)
            _posts.Remove(post);
        return Task.FromResult(post != null);
    }

    public Task<List<Post>> GetScheduledPostsAsync() => Task.FromResult(_posts.Where(p => p.ScheduledFor.HasValue).ToList());
}

public class MockPlatformService : IPlatformService
{
    private List<SocialAccount> _accounts = new();

    public MockPlatformService()
    {
        InitializeMockData();
    }

    private void InitializeMockData()
    {
        _accounts = new()
        {
            new SocialAccount { Platform = "Twitter", Username = "@socialmind", Followers = 1500, IsConnected = true },
            new SocialAccount { Platform = "LinkedIn", Username = "SocialMind", Followers = 2300, IsConnected = true },
            new SocialAccount { Platform = "Instagram", Username = "socialmind_ai", Followers = 890, IsConnected = false }
        };
    }

    public Task<List<SocialAccount>> GetAccountsAsync() => Task.FromResult(_accounts);

    public Task<SocialAccount> ConnectAccountAsync(string platform, string token)
    {
        var account = new SocialAccount { Platform = platform, IsConnected = true };
        _accounts.Add(account);
        return Task.FromResult(account);
    }

    public Task<bool> DisconnectAccountAsync(string accountId)
    {
        var account = _accounts.FirstOrDefault(a => a.Id == accountId);
        if (account != null)
            account.IsConnected = false;
        return Task.FromResult(account != null);
    }

    public Task<bool> TestConnectionAsync(string accountId) => Task.FromResult(true);
}

public class MockAnalyticsService : IAnalyticsService
{
    public Task<List<Analytics>> GetAnalyticsAsync(DateTime from, DateTime to)
    {
        return Task.FromResult(new List<Analytics>
        {
            new() { Platform = "Twitter", Impressions = 5000, Likes = 125, Comments = 42, Shares = 18 },
            new() { Platform = "LinkedIn", Impressions = 8000, Likes = 200, Comments = 85, Shares = 35 }
        });
    }

    public Task<Analytics> GetPlatformAnalyticsAsync(string platform, DateTime date)
    {
        return Task.FromResult(new Analytics
        {
            Platform = platform,
            Impressions = 10000,
            Engagements = 500,
            EngagementRate = 5.0
        });
    }

    public Task<Dictionary<string, int>> GetTopPostsAsync()
    {
        return Task.FromResult(new Dictionary<string, int>
        {
            { "Post 1", 1200 },
            { "Post 2", 890 },
            { "Post 3", 756 }
        });
    }

    public Task<List<string>> GetTrendingHashtagsAsync()
    {
        return Task.FromResult(new List<string> { "#AI", "#SocialMedia", "#Marketing", "#Tech" });
    }
}

public class MockAIService : IAIService
{
    public Task<AIGenerationResponse> GenerateContentAsync(AIGenerationRequest request)
    {
        return Task.FromResult(new AIGenerationResponse
        {
            Suggestions = new()
            {
                $"Exciting news about {request.Topic}! 🚀",
                $"Discover the {request.Topic} revolution 💡",
                $"Transform your {request.Topic} strategy today ✨"
            },
            Hashtags = new() { "#AI", "#Innovation", "#Future" }
        });
    }

    public Task<List<string>> GenerateHashtagsAsync(string content)
    {
        return Task.FromResult(new List<string> { "#Social", "#Marketing", "#AI", "#Content" });
    }

    public Task<string> OptimizeContentAsync(string content, string platform)
    {
        return Task.FromResult($"Optimized for {platform}: {content}");
    }
}

public class MockScheduleService : IScheduleService
{
    public Task<List<Post>> GetUpcomingPostsAsync(int days) => Task.FromResult(new List<Post>());
    public Task<bool> SchedulePostAsync(string postId, DateTime scheduledTime) => Task.FromResult(true);
    public Task<List<Post>> GetOptimalPostTimesAsync() => Task.FromResult(new List<Post>());
}

public class MockCampaignService : ICampaignService
{
    public Task<List<Campaign>> GetCampaignsAsync() => Task.FromResult(new List<Campaign>());
    public Task<Campaign> CreateCampaignAsync(Campaign campaign) => Task.FromResult(campaign);
    public Task<Campaign> UpdateCampaignAsync(Campaign campaign) => Task.FromResult(campaign);
    public Task<bool> DeleteCampaignAsync(string campaignId) => Task.FromResult(true);
}

public class MockSettingsService : ISettingsService
{
    private Dictionary<string, string> _settings = new();

    public Task<Dictionary<string, string>> GetSettingsAsync() => Task.FromResult(_settings);
    public Task<bool> UpdateSettingAsync(string key, string value)
    {
        _settings[key] = value;
        return Task.FromResult(true);
    }
    public Task<string?> GetSettingAsync(string key) => Task.FromResult(_settings.TryGetValue(key, out var value) ? value : null);
}
