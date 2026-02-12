using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialMind.Web.Components;
using SocialMind.Web.Data;
using SocialMind.Shared.Models;
using SocialMind.Shared.Services;
using SocialMind.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    // Use SQLite on Linux, SQL Server on Windows (for compatibility)
    if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
    {
        options.UseSqlite(connectionString);
    }
    else
    {
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });
    }
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // Şimdilik false, email servisi ekleyince true yapacağız
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add HttpClient Factory
builder.Services.AddHttpClient("MicrosoftFoundry", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AIProviders:MicrosoftFoundry:BaseUrl"]
        ?? "https://models.inference.ai.azure.com");
    client.Timeout = TimeSpan.FromSeconds(120);
});

builder.Services.AddHttpClient("AzureOpenAI", client =>
{
    var endpoint = builder.Configuration["AIProviders:AzureOpenAI:Endpoint"];
    if (!string.IsNullOrEmpty(endpoint))
    {
        client.BaseAddress = new Uri(endpoint);
    }
    client.Timeout = TimeSpan.FromSeconds(120);
});

// Add device-specific services used by the SocialMind.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

// Add Stripe Payment Service
builder.Services.AddScoped<StripePaymentService>();

// Add OAuth Services - Sosyal medya entegrasyonu
builder.Services.AddHttpClient<InstagramOAuthService>();
builder.Services.AddHttpClient<YouTubeOAuthService>();
builder.Services.AddHttpClient<TikTokOAuthService>();
builder.Services.AddScoped<IOAuthService>(sp => sp.GetRequiredService<InstagramOAuthService>()); // Default

// Add Email Service
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

// Add Usage Tracking Service
builder.Services.AddScoped<IUsageTrackingService, UsageTrackingService>();

// Add Controllers for API endpoints
builder.Services.AddControllers();

// Add Cascade Authentication State
builder.Services.AddCascadingAuthenticationState();

// Add SocialMind services
var enableMockServices = builder.Configuration.GetValue<bool>("AppSettings:EnableMockServices", true);

if (enableMockServices)
{
    // Mock implementations (Development)
    builder.Services.AddScoped<IPostService, MockPostService>();
    builder.Services.AddScoped<IPlatformService, MockPlatformService>();
    builder.Services.AddScoped<IAnalyticsService, MockAnalyticsService>();
    builder.Services.AddScoped<IAIService, MockAIService>();
    builder.Services.AddScoped<IScheduleService, MockScheduleService>();
    builder.Services.AddScoped<ICampaignService, MockCampaignService>();
    builder.Services.AddScoped<ISettingsService, MockSettingsService>();
}
else
{
    // Real implementations (Production)
    builder.Services.AddScoped<IPostService, MockPostService>(); // TODO: Real implementation
    builder.Services.AddScoped<IPlatformService, MockPlatformService>(); // TODO: Real implementation
    builder.Services.AddScoped<IAnalyticsService, MockAnalyticsService>(); // TODO: Real implementation

    // AI Services - Provider seçimine göre real implementations
    builder.Services.AddScoped<IAIService, MicrosoftFoundryAIService>(); // Ya da AzureOpenAIService

    builder.Services.AddScoped<IScheduleService, MockScheduleService>(); // TODO: Real implementation
    builder.Services.AddScoped<ICampaignService, MockCampaignService>(); // TODO: Real implementation
    builder.Services.AddScoped<ISettingsService, MockSettingsService>(); // TODO: Real implementation
}

// Named AI Service registrations (Provider'a göre seçim için)
builder.Services.AddKeyedScoped<IAIService, MicrosoftFoundryAIService>("MicrosoftFoundry");
builder.Services.AddKeyedScoped<IAIService, AzureOpenAIService>("AzureOpenAI");
builder.Services.AddKeyedScoped<IAIService, MockAIService>("Mock");

var app = builder.Build();

// Database migration ve seed (Development)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        // Seed admin role and user
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        await SeedAdminData(roleManager, userManager, context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database migration veya seeding sırasında bir hata oluştu.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(
        typeof(SocialMind.Shared._Imports).Assembly);

// Logout endpoint
app.MapPost("/logout", async (SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/");
}).RequireAuthorization();

// Stripe webhook endpoint
app.MapPost("/webhook/stripe", async (HttpRequest request, StripePaymentService stripeService) =>
{
    var json = await new StreamReader(request.Body).ReadToEndAsync();
    var signature = request.Headers["Stripe-Signature"].ToString();

    var success = await stripeService.HandleWebhookAsync(json, signature);
    return success ? Results.Ok() : Results.BadRequest();
});

// OAuth endpoints
app.MapControllers();

app.Run();

// Seed Admin Data
static async Task SeedAdminData(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
{
    // Create Admin role if it doesn't exist
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // Create default admin user if it doesn't exist
    var adminEmail = "admin@socialmind.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "Admin",
            LastName = "User",
            IsActive = true
        };

        var result = await userManager.CreateAsync(adminUser, "Admin123!");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");

            // Create Free plan for admin
            var freePlan = await context.SubscriptionPlans.FirstOrDefaultAsync(sp => sp.Name == "Free");
            if (freePlan != null)
            {
                var subscription = new UserSubscription
                {
                    UserId = adminUser.Id,
                    PlanId = freePlan.Id,
                    Status = SubscriptionStatus.Active,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(1)
                };

                context.UserSubscriptions.Add(subscription);
                await context.SaveChangesAsync();
            }
        }
    }
}
