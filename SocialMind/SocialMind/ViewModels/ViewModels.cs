using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SocialMind.Shared.Models;
using SocialMind.Shared.Services;

namespace SocialMind.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IPostService _postService;
    private readonly IPlatformService _platformService;
    private readonly IAnalyticsService _analyticsService;

    [ObservableProperty]
    private List<Post> recentPosts = new();

    [ObservableProperty]
    private List<SocialAccount> connectedAccounts = new();

    [ObservableProperty]
    private int totalPosts;

    [ObservableProperty]
    private int totalFollowers;

    [ObservableProperty]
    private double engagementRate;

    [ObservableProperty]
    private bool isLoading = true;

    public DashboardViewModel(IPostService postService, IPlatformService platformService, IAnalyticsService analyticsService)
    {
        _postService = postService;
        _platformService = platformService;
        _analyticsService = analyticsService;
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;

            var postsTask = _postService.GetPostsAsync();
            var accountsTask = _platformService.GetAccountsAsync();
            var analyticsTask = _analyticsService.GetAnalyticsAsync(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);

            await Task.WhenAll(postsTask, accountsTask, analyticsTask);

            RecentPosts = (await postsTask).OrderByDescending(p => p.CreatedAt).Take(5).ToList();
            ConnectedAccounts = (await accountsTask).Where(a => a.IsConnected).ToList();

            var analytics = await analyticsTask;
            TotalFollowers = (int)ConnectedAccounts.Sum(a => a.Followers);
            TotalPosts = RecentPosts.Count;
            EngagementRate = analytics.Sum(a => a.EngagementRate) / (analytics.Count > 0 ? analytics.Count : 1);
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public partial class CreatePostViewModel : ObservableObject
{
    private readonly IPostService _postService;
    private readonly IAIService _aiService;

    [ObservableProperty]
    private string postContent = string.Empty;

    [ObservableProperty]
    private List<string> selectedPlatforms = new();

    [ObservableProperty]
    private DateTime? scheduledTime;

    [ObservableProperty]
    private List<string> aiSuggestions = new();

    [ObservableProperty]
    private List<string> suggestedHashtags = new();

    [ObservableProperty]
    private bool isGenerating;

    public CreatePostViewModel(IPostService postService, IAIService aiService)
    {
        _postService = postService;
        _aiService = aiService;
    }

    [RelayCommand]
    public async Task GenerateAIContentAsync()
    {
        try
        {
            IsGenerating = true;

            var request = new AIGenerationRequest
            {
                Topic = "Social media engagement",
                Tone = "Professional",
                Platforms = SelectedPlatforms
            };

            var response = await _aiService.GenerateContentAsync(request);
            AiSuggestions = response.Suggestions;
            SuggestedHashtags = response.Hashtags;
        }
        finally
        {
            IsGenerating = false;
        }
    }

    [RelayCommand]
    public async Task PublishPostAsync()
    {
        try
        {
            isGenerating = true;

            var post = new Post
            {
                Content = PostContent,
                Platforms = SelectedPlatforms,
                ScheduledFor = ScheduledTime
            };

            await _postService.CreatePostAsync(post);

            PostContent = string.Empty;
            SelectedPlatforms.Clear();
            ScheduledTime = null;

            await Application.Current!.MainPage!.DisplayAlert("Success", "Post created successfully!", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to create post: {ex.Message}", "OK");
        }
        finally
        {
            isGenerating = false;
        }
    }
}

public partial class AnalyticsViewModel : ObservableObject
{
    private readonly IAnalyticsService _analyticsService;

    [ObservableProperty]
    private List<Analytics> analyticsData = new();

    [ObservableProperty]
    private Dictionary<string, int> topPosts = new();

    [ObservableProperty]
    private List<string> trendingHashtags = new();

    [ObservableProperty]
    private bool isLoading;

    public AnalyticsViewModel(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [RelayCommand]
    public async Task LoadAnalyticsAsync()
    {
        try
        {
            IsLoading = true;

            var dataTask = _analyticsService.GetAnalyticsAsync(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
            var topTask = _analyticsService.GetTopPostsAsync();
            var hashtagTask = _analyticsService.GetTrendingHashtagsAsync();

            await Task.WhenAll(dataTask, topTask, hashtagTask);

            AnalyticsData = await dataTask;
            TopPosts = await topTask;
            TrendingHashtags = await hashtagTask;
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public partial class ScheduleViewModel : ObservableObject
{
    private readonly IScheduleService _scheduleService;

    [ObservableProperty]
    private List<Post> upcomingPosts = new();

    [ObservableProperty]
    private bool isLoading;

    public ScheduleViewModel(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [RelayCommand]
    public async Task LoadScheduleAsync()
    {
        try
        {
            IsLoading = true;
            UpcomingPosts = await _scheduleService.GetUpcomingPostsAsync(7);
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public partial class SocialDashboardViewModel : ObservableObject
{
    private readonly IPlatformService _platformService;

    [ObservableProperty]
    private List<SocialAccount> socialAccounts = new();

    [ObservableProperty]
    private bool isLoading;

    public SocialDashboardViewModel(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    [RelayCommand]
    public async Task LoadAccountsAsync()
    {
        try
        {
            IsLoading = true;
            SocialAccounts = await _platformService.GetAccountsAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task DisconnectAccountAsync(string accountId)
    {
        try
        {
            await _platformService.DisconnectAccountAsync(accountId);
            SocialAccounts = await _platformService.GetAccountsAsync();
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to disconnect: {ex.Message}", "OK");
        }
    }
}
