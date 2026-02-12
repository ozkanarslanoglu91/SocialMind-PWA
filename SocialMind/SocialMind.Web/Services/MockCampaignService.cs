using SocialMind.Shared.Models;
using SocialMind.Shared.Services;

namespace SocialMind.Web.Services;

/// <summary>
/// Mock Campaign servisi - test ve geliştirme için
/// </summary>
public class MockCampaignService : ICampaignService
{
    private readonly List<CampaignAnalytics> _campaigns = new();
    private readonly Random _random = new();

    public MockCampaignService()
    {
        InitializeSampleData();
    }

    private void InitializeSampleData()
    {
        _campaigns.Add(new CampaignAnalytics
        {
            CampaignId = "1",
            CampaignName = "Yaz Kampanyası 2026",
            BudgetSpent = 1500.00m,
            Impressions = 125000,
            Clicks = 5200,
            CPC = 0.29m,
            CPM = 12.00m,
            ROI = 3.5m,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(30),
            PlatformData = new List<PlatformCampaignData>
            {
                new PlatformCampaignData
                {
                    Platform = SocialPlatform.Instagram,
                    BudgetAllocated = 600m,
                    Impressions = 50000,
                    Clicks = 2100,
                    Conversions = 87,
                    ROAS = 4.2m
                },
                new PlatformCampaignData
                {
                    Platform = SocialPlatform.Facebook,
                    BudgetAllocated = 500m,
                    Impressions = 45000,
                    Clicks = 1800,
                    Conversions = 72,
                    ROAS = 3.8m
                },
                new PlatformCampaignData
                {
                    Platform = SocialPlatform.Twitter,
                    BudgetAllocated = 400m,
                    Impressions = 30000,
                    Clicks = 1300,
                    Conversions = 45,
                    ROAS = 2.8m
                }
            }
        });
    }

    public Task<CampaignAnalytics> CreateCampaignAsync(CampaignAnalytics campaign)
    {
        campaign.CampaignId = Guid.NewGuid().ToString();
        _campaigns.Add(campaign);
        return Task.FromResult(campaign);
    }

    public Task<CampaignAnalytics> UpdateCampaignAsync(CampaignAnalytics campaign)
    {
        var existing = _campaigns.FirstOrDefault(c => c.CampaignId == campaign.CampaignId);
        if (existing != null)
        {
            var index = _campaigns.IndexOf(existing);
            _campaigns[index] = campaign;
        }
        return Task.FromResult(campaign);
    }

    public Task<bool> DeleteCampaignAsync(string campaignId)
    {
        var campaign = _campaigns.FirstOrDefault(c => c.CampaignId == campaignId);
        if (campaign != null)
        {
            _campaigns.Remove(campaign);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<CampaignAnalytics?> GetCampaignAsync(string campaignId)
    {
        var campaign = _campaigns.FirstOrDefault(c => c.CampaignId == campaignId);
        return Task.FromResult(campaign);
    }

    public Task<List<CampaignAnalytics>> GetAllCampaignsAsync()
    {
        return Task.FromResult(_campaigns.ToList());
    }

    public Task<List<CampaignAnalytics>> GetActiveCampaignsAsync()
    {
        var now = DateTime.UtcNow;
        var activeCampaigns = _campaigns
            .Where(c => c.StartDate <= now && c.EndDate >= now)
            .ToList();
        return Task.FromResult(activeCampaigns);
    }

    public Task<CampaignAnalytics> GetCampaignPerformanceAsync(string campaignId)
    {
        var campaign = _campaigns.FirstOrDefault(c => c.CampaignId == campaignId);
        if (campaign != null)
        {
            // Performans metriklerini güncelle
            campaign.Impressions = _random.Next(50000, 200000);
            campaign.Clicks = _random.Next(2000, 10000);
            campaign.CPC = Math.Round((decimal)_random.NextDouble() * 2, 2);
            campaign.CPM = Math.Round((decimal)_random.NextDouble() * 20, 2);
            campaign.ROI = Math.Round((decimal)_random.NextDouble() * 5, 1);

            return Task.FromResult(campaign);
        }

        return Task.FromResult(new CampaignAnalytics());
    }

    public Task<List<CampaignAnalytics>> GetCampaignsByPlatformAsync(SocialPlatform platform)
    {
        var campaigns = _campaigns
            .Where(c => c.PlatformData.Any(pd => pd.Platform == platform))
            .ToList();
        return Task.FromResult(campaigns);
    }
}
