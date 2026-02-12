using Microsoft.EntityFrameworkCore;
using SocialMind.Shared.Models;
using SocialMind.Web.Data;

namespace SocialMind.Web.Services;

public interface IUsageTrackingService
{
    Task<bool> CanCreatePostAsync(string userId);
    Task<bool> CanConnectPlatformAsync(string userId);
    Task<bool> CanUseAIAsync(string userId);
    Task TrackPostCreationAsync(string userId, string platform);
    Task TrackAIUsageAsync(string userId);
    Task<UsageStatistics> GetUsageStatsAsync(string userId);
    Task<SubscriptionLimits> GetUserLimitsAsync(string userId);
}

public class UsageTrackingService : IUsageTrackingService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UsageTrackingService> _logger;

    public UsageTrackingService(
        ApplicationDbContext context,
        ILogger<UsageTrackingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> CanCreatePostAsync(string userId)
    {
        var limits = await GetUserLimitsAsync(userId);
        var stats = await GetUsageStatsAsync(userId);

        if (limits.PostsPerMonth == -1) // Unlimited
            return true;

        return stats.PostsCreated < limits.PostsPerMonth;
    }

    public async Task<bool> CanConnectPlatformAsync(string userId)
    {
        var limits = await GetUserLimitsAsync(userId);
        var connectedCount = await _context.ConnectedAccounts
            .Where(ca => ca.UserId == userId)
            .CountAsync();

        if (limits.ConnectedAccounts == -1) // Unlimited
            return true;

        return connectedCount < limits.ConnectedAccounts;
    }

    public async Task<bool> CanUseAIAsync(string userId)
    {
        var limits = await GetUserLimitsAsync(userId);
        var stats = await GetUsageStatsAsync(userId);

        if (limits.AIGenerationsPerMonth == -1) // Unlimited
            return true;

        return stats.AIGenerations < limits.AIGenerationsPerMonth;
    }

    public async Task TrackPostCreationAsync(string userId, string platform)
    {
        try
        {
            var stats = await GetStatsEntityAsync(userId);

            stats.PostsCreated++;
            stats.UpdatedAt = DateTime.UtcNow;

            if (stats.PostsByPlatform == null)
                stats.PostsByPlatform = new Dictionary<string, int>();

            if (stats.PostsByPlatform.ContainsKey(platform))
                stats.PostsByPlatform[platform]++;
            else
                stats.PostsByPlatform[platform] = 1;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking post creation for user {UserId}", userId);
        }
    }

    public async Task TrackAIUsageAsync(string userId)
    {
        try
        {
            var stats = await GetStatsEntityAsync(userId);

            stats.AIGenerations++;
            stats.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking AI usage for user {UserId}", userId);
        }
    }

    public async Task<UsageStatistics> GetUsageStatsAsync(string userId)
    {
        var currentDate = DateTime.UtcNow;
        var stats = await _context.UsageStatistics
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Year == currentDate.Year && s.Month == currentDate.Month);

        if (stats == null)
        {
            stats = new UsageStatistics
            {
                UserId = userId,
                Year = currentDate.Year,
                Month = currentDate.Month,
                PostsCreated = 0,
                AIGenerations = 0,
                PostsByPlatform = new Dictionary<string, int>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        return stats;
    }

    public async Task<SubscriptionLimits> GetUserLimitsAsync(string userId)
    {
        var subscription = await _context.UserSubscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active);

        if (subscription == null)
        {
            // Default to Free plan limits
            return new SubscriptionLimits
            {
                PostsPerMonth = 10,
                AIGenerationsPerMonth = 5,
                ConnectedAccounts = 2,
                HasAnalytics = false,
                HasScheduling = false
            };
        }

        // Map plan name to limits
        return subscription.Plan.Name switch
        {
            "Free" => new SubscriptionLimits
            {
                PostsPerMonth = 10,
                AIGenerationsPerMonth = 5,
                ConnectedAccounts = 2,
                HasAnalytics = false,
                HasScheduling = false
            },
            "Pro" => new SubscriptionLimits
            {
                PostsPerMonth = 50,
                AIGenerationsPerMonth = 100,
                ConnectedAccounts = 10,
                HasAnalytics = true,
                HasScheduling = true
            },
            "Business" => new SubscriptionLimits
            {
                PostsPerMonth = -1, // Unlimited
                AIGenerationsPerMonth = -1, // Unlimited
                ConnectedAccounts = -1, // Unlimited
                HasAnalytics = true,
                HasScheduling = true
            },
            _ => new SubscriptionLimits
            {
                PostsPerMonth = 10,
                AIGenerationsPerMonth = 5,
                ConnectedAccounts = 2,
                HasAnalytics = false,
                HasScheduling = false
            }
        };
    }

    private async Task<UsageStatistics> GetStatsEntityAsync(string userId)
    {
        var currentDate = DateTime.UtcNow;

        var stats = await _context.UsageStatistics
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Year == currentDate.Year && s.Month == currentDate.Month);

        if (stats == null)
        {
            stats = new UsageStatistics
            {
                UserId = userId,
                Year = currentDate.Year,
                Month = currentDate.Month,
                PostsCreated = 0,
                AIGenerations = 0,
                PostsByPlatform = new Dictionary<string, int>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.UsageStatistics.Add(stats);
        }

        return stats;
    }
}

public class SubscriptionLimits
{
    public int PostsPerMonth { get; set; }
    public int AIGenerationsPerMonth { get; set; }
    public int ConnectedAccounts { get; set; }
    public bool HasAnalytics { get; set; }
    public bool HasScheduling { get; set; }
}
