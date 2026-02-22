using SocialMind.ViewModels;

namespace SocialMind.Views;

public partial class CreatePostPage : ContentPage
{
	public CreatePostPage()
	{
		InitializeComponent();
		BindingContext = new CreatePostViewModel(
			Application.Current!.Handler.MauiContext!.Services.GetService<IPostService>()!,
			Application.Current!.Handler.MauiContext!.Services.GetService<IAIService>()!
		);
	}
}
