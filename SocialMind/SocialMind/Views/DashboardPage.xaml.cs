using SocialMind.ViewModels;

namespace SocialMind.Views;

public partial class DashboardPage : ContentPage
{
	public DashboardPage()
	{
		InitializeComponent();
		BindingContext = new DashboardViewModel(
			Application.Current!.Handler.MauiContext!.Services.GetService<IPostService>()!,
			Application.Current!.Handler.MauiContext!.Services.GetService<IPlatformService>()!,
			Application.Current!.Handler.MauiContext!.Services.GetService<IAnalyticsService>()!
		);
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		if (BindingContext is DashboardViewModel vm)
		{
			await vm.LoadDataCommand.ExecuteAsync(null);
		}
	}
}
