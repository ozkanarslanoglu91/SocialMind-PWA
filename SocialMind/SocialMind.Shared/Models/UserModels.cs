using Microsoft.AspNetCore.Identity;

namespace SocialMind.Shared.Models;

/// <summary>
/// Uygulama kullanıcısı
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Subscription ilişkisi
    public string? SubscriptionId { get; set; }
    public UserSubscription? Subscription { get; set; }

    // Connected accounts ilişkisi
    public ICollection<ConnectedAccount> ConnectedAccounts { get; set; } = [];

    // Posts ilişkisi
    public ICollection<Post> Posts { get; set; } = [];

    // Settings
    public string Timezone { get; set; } = "UTC";
    public string Language { get; set; } = "tr";
    public string? DefaultAIProvider { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}

/// <summary>
/// Kullanıcı subscription planı
/// </summary>
public class UserSubscription
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public string PlanId { get; set; } = string.Empty;
    public SubscriptionPlan? Plan { get; set; }

    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trial;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Payment provider bilgileri
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }

    // Kullanım limitleri
    public int PostsThisMonth { get; set; } = 0;
    public int AIGenerationsThisMonth { get; set; } = 0;
    public DateTime LastResetDate { get; set; } = DateTime.UtcNow;

    public bool IsActive => Status == SubscriptionStatus.Active || Status == SubscriptionStatus.Trial;
    public bool IsInTrialPeriod => Status == SubscriptionStatus.Trial &&
                                   TrialEndDate.HasValue &&
                                   TrialEndDate.Value > DateTime.UtcNow;
}

/// <summary>
/// Subscription planı
/// </summary>
public class SubscriptionPlan
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Pricing
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
    public string Currency { get; set; } = "USD";

    // Stripe bilgileri
    public string? StripePriceIdMonthly { get; set; }
    public string? StripePriceIdYearly { get; set; }
    public string? StripeProductId { get; set; }

    // Limitler
    public int MaxPosts { get; set; }
    public int MaxAIGenerations { get; set; }
    public int MaxConnectedAccounts { get; set; }
    public int MaxScheduledPosts { get; set; }

    // Özellikler
    public bool HasAdvancedAnalytics { get; set; }
    public bool HasTeamCollaboration { get; set; }
    public bool HasPrioritySupport { get; set; }
    public bool HasCustomBranding { get; set; }
    public bool HasAPIAccess { get; set; }

    // Metadata
    public int SortOrder { get; set; }
    public bool IsPopular { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // İlişkiler
    public ICollection<UserSubscription> Subscriptions { get; set; } = [];
}

/// <summary>
/// Subscription durumu
/// </summary>
public enum SubscriptionStatus
{
    Trial = 0,
    Active = 1,
    PastDue = 2,
    Cancelled = 3,
    Expired = 4
}

/// <summary>
/// Payment transaction
/// </summary>
public class PaymentTransaction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentStatus Status { get; set; }

    // Stripe bilgileri
    public string? StripePaymentIntentId { get; set; }
    public string? StripeChargeId { get; set; }
    public string? StripeInvoiceId { get; set; }

    // Detaylar
    public string? Description { get; set; }
    public string? SubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string? FailureReason { get; set; }

    // Metadata
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Payment durumu
/// </summary>
public enum PaymentStatus
{
    Pending = 0,
    Processing = 1,
    Succeeded = 2,
    Failed = 3,
    Refunded = 4,
    Cancelled = 5
}

/// <summary>
/// Kullanım istatistiği
/// </summary>
public class UsageStatistics
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public int Year { get; set; }
    public int Month { get; set; }

    // Kullanım sayıları
    public int PostsCreated { get; set; }
    public int PostsPublished { get; set; }
    public int AIGenerations { get; set; }
    public int AITokensUsed { get; set; }

    // Platform dağılımı
    public Dictionary<string, int> PostsByPlatform { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Varsayılan subscription planları
/// </summary>
public static class DefaultSubscriptionPlans
{
    public static readonly SubscriptionPlan Free = new()
    {
        Id = "free",
        Name = "Free",
        Description = "Başlamak için ideal",
        MonthlyPrice = 0,
        YearlyPrice = 0,
        MaxPosts = 10,
        MaxAIGenerations = 50,
        MaxConnectedAccounts = 2,
        MaxScheduledPosts = 5,
        HasAdvancedAnalytics = false,
        HasTeamCollaboration = false,
        HasPrioritySupport = false,
        HasCustomBranding = false,
        HasAPIAccess = false,
        SortOrder = 1,
        IsPopular = false
    };

    public static readonly SubscriptionPlan Pro = new()
    {
        Id = "pro",
        Name = "Pro",
        Description = "Profesyoneller için",
        MonthlyPrice = 29.99m,
        YearlyPrice = 299.99m, // ~16% indirim
        MaxPosts = 100,
        MaxAIGenerations = 1000,
        MaxConnectedAccounts = 10,
        MaxScheduledPosts = 50,
        HasAdvancedAnalytics = true,
        HasTeamCollaboration = false,
        HasPrioritySupport = true,
        HasCustomBranding = false,
        HasAPIAccess = false,
        SortOrder = 2,
        IsPopular = true
    };

    public static readonly SubscriptionPlan Business = new()
    {
        Id = "business",
        Name = "Business",
        Description = "Büyük kurumlar için",
        MonthlyPrice = 99.99m,
        YearlyPrice = 999.99m, // ~16% indirim
        MaxPosts = -1, // Unlimited
        MaxAIGenerations = -1, // Unlimited
        MaxConnectedAccounts = 50,
        MaxScheduledPosts = -1, // Unlimited
        HasAdvancedAnalytics = true,
        HasTeamCollaboration = true,
        HasPrioritySupport = true,
        HasCustomBranding = true,
        HasAPIAccess = true,
        SortOrder = 3,
        IsPopular = false
    };

    public static IEnumerable<SubscriptionPlan> All => new[] { Free, Pro, Business };
}
