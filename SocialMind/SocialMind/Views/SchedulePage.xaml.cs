using SocialMind.ViewModels;

namespace SocialMind.Views;

public partial class SchedulePage : ContentPage
{
    public SchedulePage()
    {
        InitializeComponent();
        BindingContext = new ScheduleViewModel(
            Application.Current!.Handler.MauiContext!.Services.GetService<IScheduleService>()!
        );
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ScheduleViewModel vm)
        {
            await vm.LoadScheduleCommand.ExecuteAsync(null);
        }
    }
}
