using SocialMind.ViewModels;

namespace SocialMind.Views;

public partial class SocialDashboardPage : ContentPage
{
    public SocialDashboardPage()
    {
        InitializeComponent();
        BindingContext = new SocialDashboardViewModel(
            Application.Current!.Handler.MauiContext!.Services.GetService<IPlatformService>()!
        );
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is SocialDashboardViewModel vm)
        {
            await vm.LoadAccountsCommand.ExecuteAsync(null);
        }
    }
}
