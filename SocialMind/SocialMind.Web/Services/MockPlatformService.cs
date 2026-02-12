using SocialMind.Shared.Models;
using SocialMind.Shared.Services;

namespace SocialMind.Web.Services;

/// <summary>
/// Mock Platform servisi - test ve geliştirme için
/// </summary>
public class MockPlatformService : IPlatformService
{
    private readonly List<ConnectedAccount> _connectedAccounts = new();

    public MockPlatformService()
    {
        InitializeSampleData();
    }

    private void InitializeSampleData()
    {
        // Örnek bağlı hesaplar
        _connectedAccounts.Add(new ConnectedAccount
        {
            Id = "1",
            Platform = SocialPlatform.Twitter,
            AccountName = "@socialmind_app",
            AccountId = "twitter_12345",
            ProfileImageUrl = "/assets/icons/twitter-icon.svg",
            ConnectedAt = DateTime.UtcNow.AddDays(-10),
            IsActive = true,
            TokenExpiresAt = DateTime.UtcNow.AddDays(30)
        });

        _connectedAccounts.Add(new ConnectedAccount
        {
            Id = "2",
            Platform = SocialPlatform.LinkedIn,
            AccountName = "SocialMind",
            AccountId = "linkedin_67890",
            ProfileImageUrl = "/assets/icons/linkedin-icon.svg",
            ConnectedAt = DateTime.UtcNow.AddDays(-5),
            IsActive = true,
            TokenExpiresAt = DateTime.UtcNow.AddDays(60)
        });
    }

    public Task<ConnectedAccount> ConnectAccountAsync(SocialPlatform platform, string authCode)
    {
        var account = new ConnectedAccount
        {
            Id = Guid.NewGuid().ToString(),
            Platform = platform,
            AccountName = $"{platform} Account",
            AccountId = $"{platform}_{Guid.NewGuid().ToString().Substring(0, 8)}",
            AuthToken = authCode,
            RefreshToken = Guid.NewGuid().ToString(),
            ConnectedAt = DateTime.UtcNow,
            TokenExpiresAt = DateTime.UtcNow.AddDays(30),
            IsActive = true
        };

        _connectedAccounts.Add(account);
        return Task.FromResult(account);
    }

    public Task<bool> DisconnectAccountAsync(string accountId)
    {
        var account = _connectedAccounts.FirstOrDefault(a => a.Id == accountId);
        if (account != null)
        {
            _connectedAccounts.Remove(account);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> RefreshTokenAsync(string accountId)
    {
        var account = _connectedAccounts.FirstOrDefault(a => a.Id == accountId);
        if (account != null)
        {
            account.RefreshToken = Guid.NewGuid().ToString();
            account.TokenExpiresAt = DateTime.UtcNow.AddDays(30);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<ConnectedAccount?> GetAccountAsync(string accountId)
    {
        var account = _connectedAccounts.FirstOrDefault(a => a.Id == accountId);
        return Task.FromResult(account);
    }

    public Task<List<ConnectedAccount>> GetConnectedAccountsAsync(SocialPlatform? platform = null)
    {
        var query = _connectedAccounts.AsQueryable();
        if (platform.HasValue)
        {
            query = query.Where(a => a.Platform == platform.Value);
        }
        return Task.FromResult(query.ToList());
    }

    public Task<List<ConnectedAccount>> GetAllConnectedAccountsAsync()
    {
        return Task.FromResult(_connectedAccounts.ToList());
    }

    public Task<PlatformConfiguration> GetPlatformConfigAsync(SocialPlatform platform)
    {
        var config = PlatformRegistry.GetPlatform(platform);
        return Task.FromResult(config ?? new PlatformConfiguration());
    }

    public Task<string> GetPlatformAuthUrlAsync(SocialPlatform platform)
    {
        var config = PlatformRegistry.GetPlatform(platform);
        var authUrl = config?.OAuthAuthorizationUrl ?? "https://example.com/oauth";

        // Gerçek OAuth URL'ini oluştur
        var redirectUri = "https://localhost:7259/oauth/callback";
        var clientId = "mock_client_id";
        var scope = "read,write";

        return Task.FromResult($"{authUrl}?client_id={clientId}&redirect_uri={redirectUri}&scope={scope}");
    }

    public Task<bool> TestConnectionAsync(string accountId)
    {
        var account = _connectedAccounts.FirstOrDefault(a => a.Id == accountId);
        return Task.FromResult(account != null && account.IsActive);
    }
}
