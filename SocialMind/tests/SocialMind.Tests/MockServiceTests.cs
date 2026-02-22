using Xunit;
using SocialMind.Shared.Models;
using SocialMind.Shared.Services;

namespace SocialMind.Tests;

public class MockServiceTests
{
    [Fact]
    public async Task MockPostService_CreatePost_ReturnsPost()
    {
        // Arrange
        var service = new MockPostService();
        var post = new Post { Content = "Test post" };

        // Act
        var result = await service.CreatePostAsync(post);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test post", result.Content);
    }

    [Fact]
    public async Task MockPostService_GetPosts_ReturnsPostList()
    {
        // Arrange
        var service = new MockPostService();

        // Act
        var result = await service.GetPostsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<List<Post>>(result);
    }

    [Fact]
    public async Task MockPlatformService_GetAccounts_ReturnsAccounts()
    {
        // Arrange
        var service = new MockPlatformService();

        // Act
        var result = await service.GetAccountsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task MockAnalyticsService_GetAnalytics_ReturnsData()
    {
        // Arrange
        var service = new MockAnalyticsService();

        // Act
        var result = await service.GetAnalyticsAsync(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task MockAIService_GenerateContent_ReturnsSuggestions()
    {
        // Arrange
        var service = new MockAIService();
        var request = new AIGenerationRequest { Topic = "Test Topic" };

        // Act
        var result = await service.GenerateContentAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Suggestions);
        Assert.NotEmpty(result.Hashtags);
    }
}
