using Microsoft.Extensions.Logging;
using SocialMind.Services;
using SocialMind.ViewModels;
using SocialMind.Views;
using SocialMind.Shared.Services;

namespace SocialMind;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services
			.AddSingleton<IPostService, MockPostService>()
			.AddSingleton<IPlatformService, MockPlatformService>()
			.AddSingleton<IAnalyticsService, MockAnalyticsService>()
			.AddSingleton<IAIService, MockAIService>()
			.AddSingleton<IScheduleService, MockScheduleService>()
			.AddSingleton<ICampaignService, MockCampaignService>()
			.AddSingleton<ISettingsService, MockSettingsService>()
			// Register ViewModels
			.AddSingleton<DashboardViewModel>()
			.AddSingleton<CreatePostViewModel>()
			.AddSingleton<AnalyticsViewModel>()
			.AddSingleton<ScheduleViewModel>()
			.AddSingleton<SocialDashboardViewModel>()
			// Register Views
			.AddSingleton<DashboardPage>()
			.AddSingleton<CreatePostPage>()
			.AddSingleton<AnalyticsPage>()
			.AddSingleton<SchedulePage>()
			.AddSingleton<SocialDashboardPage>()
			// Add HttpClient
			.AddHttpClient();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
