using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMind.Web.Data;
using SocialMind.Web.Services;
using SocialMind.Shared.Models;

namespace SocialMind.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OAuthController : ControllerBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OAuthController> _logger;

    public OAuthController(
        IHttpContextAccessor httpContextAccessor,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IServiceProvider serviceProvider,
        ILogger<OAuthController> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
        _userManager = userManager;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// OAuth yetkilendirme URL'sini al
    /// </summary>
    [HttpGet("authorize/{platform}")]
    public async Task<IActionResult> GetAuthorizationUrl(string platform)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        try
        {
            var service = GetOAuthService(platform);
            var authUrl = service.GetAuthorizationUrl(platform, user.Id);
            return Ok(new { authorizationUrl = authUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OAuth authorization URL generation failed for platform: {Platform}", platform);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// OAuth callback - Platform redirect ediyor
    /// </summary>
    [HttpGet("callback/{platform}")]
    [AllowAnonymous]
    public async Task<IActionResult> OAuthCallback(string platform, [FromQuery] string code, [FromQuery] string state)
    {
        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            return BadRequest("Code ve State parametreleri gerekli");

        try
        {
            // State'ten userId'yi çıkar
            var userIdBytes = Convert.FromBase64String(state);
            var userId = System.Text.Encoding.UTF8.GetString(userIdBytes);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return BadRequest("Kullanıcı bulunamadı");

            // Token exchange et
            var service = GetOAuthService(platform);
            var tokenResponse = await service.ExchangeCodeForTokenAsync(platform, code);

            // Profile bilgilerini çek
            var profile = await service.GetProfileAsync(platform, tokenResponse.AccessToken);

            // Mevcut connected account'ı kontrol et
            var existing = await _context.ConnectedAccounts
                .FirstOrDefaultAsync(ca => ca.UserId == userId && ca.Platform.ToString() == platform);

            if (existing != null)
            {
                // Update
                existing.AuthToken = tokenResponse.AccessToken;
                existing.RefreshToken = tokenResponse.RefreshToken ?? existing.RefreshToken;
                existing.TokenExpiresAt = tokenResponse.ExpiresAt;
                existing.ProfileImageUrl = profile.ProfileImageUrl;
                existing.IsActive = true;
                _context.ConnectedAccounts.Update(existing);
            }
            else
            {
                // Create new
                var metadata = new Dictionary<string, object>();
                if (tokenResponse.Metadata != null)
                {
                    foreach (var kvp in tokenResponse.Metadata)
                        metadata[kvp.Key] = kvp.Value;
                }

                var account = new ConnectedAccount
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Platform = Enum.Parse<SocialPlatform>(platform, ignoreCase: true),
                    AccountName = profile.Username ?? profile.DisplayName ?? "Unknown",
                    AccountId = profile.Id,
                    ProfileImageUrl = profile.ProfileImageUrl,
                    AuthToken = tokenResponse.AccessToken,
                    RefreshToken = tokenResponse.RefreshToken ?? string.Empty,
                    TokenExpiresAt = tokenResponse.ExpiresAt,
                    ConnectedAt = DateTime.UtcNow,
                    IsActive = true,
                    Metadata = metadata
                };
                _context.ConnectedAccounts.Add(account);
            }

            await _context.SaveChangesAsync();

            // UI'ye redirect et - başarı mesajı ile
            return Redirect($"/connected-accounts?platform={platform}&success=true");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OAuth callback failed for platform: {Platform}", platform);
            return Redirect($"/connected-accounts?platform={platform}&error={Uri.EscapeDataString(ex.Message)}");
        }
    }

    /// <summary>
    /// Connected account'ı bağlantıdan çıkar
    /// </summary>
    [HttpPost("disconnect/{accountId}")]
    public async Task<IActionResult> Disconnect(string accountId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var account = await _context.ConnectedAccounts
            .FirstOrDefaultAsync(ca => ca.Id == accountId && ca.UserId == user.Id);

        if (account == null)
            return NotFound();

        _context.ConnectedAccounts.Remove(account);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Hesap bağlantısı başarıyla kaldırıldı" });
    }

    /// <summary>
    /// User'ın connected account'larını listele
    /// </summary>
    [HttpGet("my-accounts")]
    public async Task<IActionResult> GetMyAccounts()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var accounts = await _context.ConnectedAccounts
            .Where(ca => ca.UserId == user.Id && ca.IsActive)
            .Select(ca => new
            {
                ca.Id,
                ca.Platform,
                ca.AccountName,
                ca.AccountId,
                ca.ProfileImageUrl,
                ca.ConnectedAt,
                TokenExpiresAt = ca.TokenExpiresAt,
                NeedsRefresh = ca.TokenExpiresAt < DateTime.UtcNow.AddDays(3)
            })
            .ToListAsync();

        return Ok(accounts);
    }

    private IOAuthService GetOAuthService(string platform)
    {
        return platform.ToLowerInvariant() switch
        {
            "instagram" => _serviceProvider.GetRequiredService<InstagramOAuthService>(),
            "youtube" => _serviceProvider.GetRequiredService<YouTubeOAuthService>(),
            "tiktok" => _serviceProvider.GetRequiredService<TikTokOAuthService>(),
            _ => throw new ArgumentException($"Platform '{platform}' is not supported")
        };
    }
}
