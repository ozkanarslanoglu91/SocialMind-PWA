using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialMind.Shared.Models;
using System.Text.Json;

namespace SocialMind.Web.Data;

/// <summary>
/// SocialMind veritabanı context'i
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<Post> Posts { get; set; }
    public DbSet<ConnectedAccount> ConnectedAccounts { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<UserSubscription> UserSubscriptions { get; set; }
    public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
    public DbSet<UsageStatistics> UsageStatistics { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        // Suppress the pending model changes warning for development
        optionsBuilder.ConfigureWarnings(w =>
            w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ApplicationUser yapılandırması
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");

            entity.HasOne(u => u.Subscription)
                .WithOne(s => s.User)
                .HasForeignKey<UserSubscription>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.ConnectedAccounts)
                .WithOne()
                .HasForeignKey(ca => ca.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.Posts)
                .WithOne()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        });

        // Post yapılandırması
        builder.Entity<Post>(entity =>
        {
            entity.ToTable("Posts");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Title).HasMaxLength(500);
            entity.Property(p => p.Content).HasMaxLength(10000);
            entity.Property(p => p.AIModelUsed).HasMaxLength(100);
            entity.Property(p => p.AIPrompt).HasMaxLength(2000);

            // JSON kolonları
            entity.Property(p => p.Platforms)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<SocialPlatform>>(v, (JsonSerializerOptions?)null) ?? new List<SocialPlatform>()
                );

            entity.Property(p => p.MediaItems)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<PostMedia>>(v, (JsonSerializerOptions?)null) ?? new List<PostMedia>()
                );

            entity.Property(p => p.Hashtags)
                .HasConversion(
                    v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => v == null ? null : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)
                );

            entity.Property(p => p.PlatformData)
                .HasConversion(
                    v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, PostPlatformData>>(v, (JsonSerializerOptions?)null)
                );

            entity.HasIndex(p => p.UserId);
            entity.HasIndex(p => p.Status);
            entity.HasIndex(p => p.CreatedAt);
            entity.HasIndex(p => p.ScheduledAt);
        });

        // ConnectedAccount yapılandırması
        builder.Entity<ConnectedAccount>(entity =>
        {
            entity.ToTable("ConnectedAccounts");
            entity.HasKey(ca => ca.Id);

            entity.Property(ca => ca.AccountName).HasMaxLength(200);
            entity.Property(ca => ca.AccountId).HasMaxLength(200);
            entity.Property(ca => ca.ProfileImageUrl).HasMaxLength(500);
            entity.Property(ca => ca.AuthToken).HasMaxLength(2000);
            entity.Property(ca => ca.RefreshToken).HasMaxLength(2000);

            entity.Property(ca => ca.Metadata)
                .HasConversion(
                    v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null)
                );

            entity.HasIndex(ca => ca.UserId);
            entity.HasIndex(ca => new { ca.UserId, ca.Platform });
        });

        // SubscriptionPlan yapılandırması
        builder.Entity<SubscriptionPlan>(entity =>
        {
            entity.ToTable("SubscriptionPlans");
            entity.HasKey(sp => sp.Id);

            entity.Property(sp => sp.Name).HasMaxLength(100).IsRequired();
            entity.Property(sp => sp.Description).HasMaxLength(500);
            entity.Property(sp => sp.Currency).HasMaxLength(3).HasDefaultValue("USD");
            entity.Property(sp => sp.MonthlyPrice).HasPrecision(10, 2);
            entity.Property(sp => sp.YearlyPrice).HasPrecision(10, 2);

            entity.HasMany(sp => sp.Subscriptions)
                .WithOne(us => us.Plan)
                .HasForeignKey(us => us.PlanId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // UserSubscription yapılandırması
        builder.Entity<UserSubscription>(entity =>
        {
            entity.ToTable("UserSubscriptions");
            entity.HasKey(us => us.Id);

            entity.Property(us => us.StripeCustomerId).HasMaxLength(200);
            entity.Property(us => us.StripeSubscriptionId).HasMaxLength(200);

            entity.HasIndex(us => us.UserId).IsUnique();
            entity.HasIndex(us => us.StripeCustomerId);
            entity.HasIndex(us => us.Status);
        });

        // PaymentTransaction yapılandırması
        builder.Entity<PaymentTransaction>(entity =>
        {
            entity.ToTable("PaymentTransactions");
            entity.HasKey(pt => pt.Id);

            entity.Property(pt => pt.Amount).HasPrecision(10, 2);
            entity.Property(pt => pt.Currency).HasMaxLength(3).HasDefaultValue("USD");
            entity.Property(pt => pt.StripePaymentIntentId).HasMaxLength(200);
            entity.Property(pt => pt.StripeChargeId).HasMaxLength(200);
            entity.Property(pt => pt.StripeInvoiceId).HasMaxLength(200);
            entity.Property(pt => pt.Description).HasMaxLength(500);
            entity.Property(pt => pt.FailureReason).HasMaxLength(1000);

            entity.Property(pt => pt.Metadata)
                .HasConversion(
                    v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null)
                );

            entity.HasIndex(pt => pt.UserId);
            entity.HasIndex(pt => pt.StripePaymentIntentId);
            entity.HasIndex(pt => pt.CreatedAt);
        });

        // UsageStatistics yapılandırması
        builder.Entity<UsageStatistics>(entity =>
        {
            entity.ToTable("UsageStatistics");
            entity.HasKey(us => us.Id);

            entity.Property(us => us.PostsByPlatform)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, int>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, int>()
                );

            entity.HasIndex(us => us.UserId);
            entity.HasIndex(us => new { us.UserId, us.Year, us.Month }).IsUnique();
        });

        // Seeding is done in Program.cs with MigrateAsync to avoid migration conflicts
    }
}
