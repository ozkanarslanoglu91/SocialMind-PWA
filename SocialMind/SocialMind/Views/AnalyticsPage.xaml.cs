using SocialMind.ViewModels;

namespace SocialMind.Views;

public partial class AnalyticsPage : ContentPage
{
    public AnalyticsPage()
    {
        InitializeComponent();
        BindingContext = new AnalyticsViewModel(
            Application.Current!.Handler.MauiContext!.Services.GetService<IAnalyticsService>()!
        );
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AnalyticsViewModel vm)
        {
            await vm.LoadAnalyticsCommand.ExecuteAsync(null);
        }
    }
}
