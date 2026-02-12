using Stripe;
using Stripe.Checkout;
using SocialMind.Shared.Models;
using Microsoft.EntityFrameworkCore;
using SocialMind.Web.Data;

namespace SocialMind.Web.Services;

/// <summary>
/// Stripe payment servisi
/// </summary>
public class StripePaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripePaymentService> _logger;
    private readonly string _stripeSecretKey;
    private readonly string _stripeWebhookSecret;

    public StripePaymentService(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<StripePaymentService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _stripeSecretKey = configuration["Stripe:SecretKey"] ?? string.Empty;
        _stripeWebhookSecret = configuration["Stripe:WebhookSecret"] ?? string.Empty;

        if (!string.IsNullOrEmpty(_stripeSecretKey))
        {
            StripeConfiguration.ApiKey = _stripeSecretKey;
        }
    }

    /// <summary>
    /// Checkout session oluştur
    /// </summary>
    public async Task<string> CreateCheckoutSessionAsync(
        string userId,
        string planId,
        bool isYearly,
        string successUrl,
        string cancelUrl)
    {
        var plan = await _context.SubscriptionPlans.FindAsync(planId);
        if (plan == null)
            throw new ArgumentException("Plan bulunamadı");

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new ArgumentException("Kullanıcı bulunamadı");

        // Stripe Customer oluştur veya getir
        var customerId = await GetOrCreateCustomerAsync(user);

        var priceId = isYearly ? plan.StripePriceIdYearly : plan.StripePriceIdMonthly;
        if (string.IsNullOrEmpty(priceId))
            throw new InvalidOperationException("Stripe price ID yapılandırılmamış");

        var options = new SessionCreateOptions
        {
            Customer = customerId,
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Price = priceId,
                    Quantity = 1,
                }
            },
            Mode = "subscription",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            Metadata = new Dictionary<string, string>
            {
                { "user_id", userId },
                { "plan_id", planId },
                { "billing_period", isYearly ? "yearly" : "monthly" }
            },
            SubscriptionData = new SessionSubscriptionDataOptions
            {
                TrialPeriodDays = plan.Id == "free" ? 0 : 14, // Free plan için trial yok
                Metadata = new Dictionary<string, string>
                {
                    { "user_id", userId },
                    { "plan_id", planId }
                }
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        return session.Url;
    }

    /// <summary>
    /// Customer portal session oluştur (subscription yönetimi için)
    /// </summary>
    public async Task<string> CreatePortalSessionAsync(string userId, string returnUrl)
    {
        var subscription = await _context.UserSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (subscription == null || string.IsNullOrEmpty(subscription.StripeCustomerId))
            throw new InvalidOperationException("Aktif subscription bulunamadı");

        var options = new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = subscription.StripeCustomerId,
            ReturnUrl = returnUrl,
        };

        var service = new Stripe.BillingPortal.SessionService();
        var session = await service.CreateAsync(options);

        return session.Url;
    }

    /// <summary>
    /// Webhook event'lerini işle
    /// </summary>
    public async Task<bool> HandleWebhookAsync(string json, string signature)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                signature,
                _stripeWebhookSecret
            );

            _logger.LogInformation("Stripe webhook alındı: {EventType}", stripeEvent.Type);

            switch (stripeEvent.Type)
            {
                case "checkout.session.completed":
                    await HandleCheckoutCompletedAsync(stripeEvent);
                    break;

                case "customer.subscription.created":
                case "customer.subscription.updated":
                    await HandleSubscriptionUpdatedAsync(stripeEvent);
                    break;

                case "customer.subscription.deleted":
                    await HandleSubscriptionDeletedAsync(stripeEvent);
                    break;

                case "invoice.payment_succeeded":
                    await HandlePaymentSucceededAsync(stripeEvent);
                    break;

                case "invoice.payment_failed":
                    await HandlePaymentFailedAsync(stripeEvent);
                    break;

                default:
                    _logger.LogInformation("Unhandled event type: {EventType}", stripeEvent.Type);
                    break;
            }

            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook hatası");
            return false;
        }
    }

    private async Task HandleCheckoutCompletedAsync(Event stripeEvent)
    {
        var session = stripeEvent.Data.Object as Session;
        if (session == null) return;

        var userId = session.Metadata["user_id"];
        var planId = session.Metadata["plan_id"];

        var plan = await _context.SubscriptionPlans.FindAsync(planId);
        if (plan == null) return;

        // Kullanıcının subscription'ını oluştur veya güncelle
        var userSubscription = await _context.UserSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (userSubscription == null)
        {
            userSubscription = new UserSubscription
            {
                UserId = userId,
                PlanId = planId,
                Status = SubscriptionStatus.Active,
                StartDate = DateTime.UtcNow,
                TrialEndDate = DateTime.UtcNow.AddDays(14),
                StripeCustomerId = session.CustomerId,
                StripeSubscriptionId = session.SubscriptionId
            };
            _context.UserSubscriptions.Add(userSubscription);
        }
        else
        {
            userSubscription.PlanId = planId;
            userSubscription.Status = SubscriptionStatus.Active;
            userSubscription.StripeCustomerId = session.CustomerId;
            userSubscription.StripeSubscriptionId = session.SubscriptionId;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Checkout completed for user {UserId}", userId);
    }

    private async Task HandleSubscriptionUpdatedAsync(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Subscription;
        if (subscription == null) return;

        var userId = subscription.Metadata["user_id"];
        var userSubscription = await _context.UserSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (userSubscription != null)
        {
            userSubscription.Status = subscription.Status switch
            {
                "active" => SubscriptionStatus.Active,
                "past_due" => SubscriptionStatus.PastDue,
                "canceled" => SubscriptionStatus.Cancelled,
                _ => userSubscription.Status
            };

            await _context.SaveChangesAsync();
        }
    }

    private async Task HandleSubscriptionDeletedAsync(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Subscription;
        if (subscription == null) return;

        var userId = subscription.Metadata["user_id"];
        var userSubscription = await _context.UserSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (userSubscription != null)
        {
            userSubscription.Status = SubscriptionStatus.Cancelled;
            userSubscription.CancelledAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private async Task HandlePaymentSucceededAsync(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        if (invoice == null) return;

        var subscriptionId = invoice.Lines?.Data?.FirstOrDefault()?.SubscriptionId;
        if (string.IsNullOrWhiteSpace(subscriptionId)) return;

        var subscription = await _context.UserSubscriptions
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == subscriptionId);

        if (subscription != null)
        {
            var payment = invoice.Payments?.Data?.FirstOrDefault()?.Payment;
            var transaction = new PaymentTransaction
            {
                UserId = subscription.UserId,
                Amount = (decimal)(invoice.AmountPaid / 100.0),
                Currency = invoice.Currency.ToUpper(),
                Status = PaymentStatus.Succeeded,
                StripePaymentIntentId = payment?.PaymentIntentId,
                StripeChargeId = payment?.ChargeId,
                StripeInvoiceId = invoice.Id,
                Description = $"Subscription payment for {subscription.PlanId}",
                SubscriptionId = subscription.Id,
                CompletedAt = DateTime.UtcNow
            };

            _context.PaymentTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Payment succeeded for subscription {SubscriptionId}", subscription.Id);
        }
    }

    private async Task HandlePaymentFailedAsync(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        if (invoice == null) return;

        var subscriptionId = invoice.Lines?.Data?.FirstOrDefault()?.SubscriptionId;
        if (string.IsNullOrWhiteSpace(subscriptionId)) return;

        var subscription = await _context.UserSubscriptions
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == subscriptionId);

        if (subscription != null)
        {
            subscription.Status = SubscriptionStatus.PastDue;

            var transaction = new PaymentTransaction
            {
                UserId = subscription.UserId,
                Amount = (decimal)(invoice.AmountDue / 100.0),
                Currency = invoice.Currency.ToUpper(),
                Status = PaymentStatus.Failed,
                StripeInvoiceId = invoice.Id,
                Description = $"Failed payment for {subscription.PlanId}",
                SubscriptionId = subscription.Id,
                FailureReason = "Payment failed"
            };

            _context.PaymentTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            _logger.LogWarning("Payment failed for subscription {SubscriptionId}", subscription.Id);
        }
    }

    private async Task<string> GetOrCreateCustomerAsync(ApplicationUser user)
    {
        var subscription = await _context.UserSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == user.Id);

        if (subscription != null && !string.IsNullOrEmpty(subscription.StripeCustomerId))
        {
            return subscription.StripeCustomerId;
        }

        var customerService = new CustomerService();
        var customer = await customerService.CreateAsync(new CustomerCreateOptions
        {
            Email = user.Email,
            Name = user.FullName,
            Metadata = new Dictionary<string, string>
            {
                { "user_id", user.Id }
            }
        });

        return customer.Id;
    }

    /// <summary>
    /// Subscription'ı iptal et
    /// </summary>
    public async Task<bool> CancelSubscriptionAsync(string userId)
    {
        var subscription = await _context.UserSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (subscription == null || string.IsNullOrEmpty(subscription.StripeSubscriptionId))
            return false;

        try
        {
            var service = new SubscriptionService();
            await service.CancelAsync(subscription.StripeSubscriptionId);

            subscription.Status = SubscriptionStatus.Cancelled;
            subscription.CancelledAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Subscription iptal hatası");
            return false;
        }
    }
}
