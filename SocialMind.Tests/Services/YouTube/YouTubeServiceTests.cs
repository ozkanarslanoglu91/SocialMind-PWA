using System.Net;
using FluentAssertions;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using SocialMind.Shared.Models;
using SocialMind.Web.Services.YouTube;

namespace SocialMind.Tests.Services.YouTube;

public class YouTubeServiceTests
{
    private readonly Mock<ILogger<YouTubeService>> _loggerMock;
    private readonly YouTubeService _service;

    public YouTubeServiceTests()
    {
        _loggerMock = new Mock<ILogger<YouTubeService>>();
        
        var mockHttpClientHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(mockHttpClientHandler.Object)
        {
            BaseAddress = new Uri("https://www.googleapis.com/youtube/v3")
        };

        _service = new YouTubeService(httpClient, _loggerMock.Object);
    }

    [Fact]
    public async Task UploadVideoAsync_WithValidToken_ReturnsSuccessResult()
    {
        // Arrange
        var accessToken = "valid_token";
        var videoPath = "test_video.mp4";
        var post = new Post { Caption = "Test video", Content = "Test content", PlatformType = "YouTube" };

        // Create a mock video file
        File.WriteAllBytes(videoPath, new byte[] { 1, 2, 3, 4, 5 });

        try
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""id"": ""abc123"",
                    ""kind"": ""youtube#video"",
                    ""etag"": ""test""
                }")
            };

            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(mockResponse);

            var httpClient = new HttpClient(mockHandler.Object);
            var service = new YouTubeService(httpClient, _loggerMock.Object);

            // Act
            var result = await service.UploadVideoAsync(accessToken, videoPath, post);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }
        finally
        {
            if (File.Exists(videoPath))
                File.Delete(videoPath);
        }
    }

    [Fact]
    public async Task UploadVideoAsync_WithEmptyToken_ReturnsFailResult()
    {
        // Arrange
        var videoPath = "test.mp4";
        var post = new Post { Caption = "Test" };

        File.WriteAllBytes(videoPath, new byte[] { 1, 2, 3 });

        try
        {
            // Act
            var result = await _service.UploadVideoAsync("", videoPath, post);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be("INVALID_TOKEN");
        }
        finally
        {
            if (File.Exists(videoPath))
                File.Delete(videoPath);
        }
    }

    [Fact]
    public async Task UploadVideoAsync_WithNonExistentFile_ReturnsFailResult()
    {
        // Arrange
        var accessToken = "valid_token";
        var videoPath = "nonexistent_video.mp4";
        var post = new Post { Caption = "Test" };

        // Act
        var result = await _service.UploadVideoAsync(accessToken, videoPath, post);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("FILE_NOT_FOUND");
    }

    [Fact]
    public async Task GetChannelInfoAsync_WithValidToken_ReturnsChannelInfo()
    {
        // Arrange
        var accessToken = "valid_token";
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(@"{
                ""items"": [{
                    ""id"": ""channel_id_123"",
                    ""snippet"": {
                        ""title"": ""My Channel"",
                        ""description"": ""Test channel""
                    },
                    ""statistics"": {
                        ""subscriberCount"": ""1000"",
                        ""videoCount"": ""50"",
                        ""viewCount"": ""100000""
                    }
                }]
            }")
        };

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);

        var httpClient = new HttpClient(mockHandler.Object);
        var service = new YouTubeService(httpClient, _loggerMock.Object);

        // Act
        var result = await service.GetChannelInfoAsync(accessToken);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Title.Should().Be("My Channel");
        result.Data.SubscriberCount.Should().Be(1000);
    }

    [Fact]
    public async Task GetChannelInfoAsync_WithEmptyToken_ReturnsFailResult()
    {
        // Act
        var result = await _service.GetChannelInfoAsync("");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_TOKEN");
    }

    [Fact]
    public async Task GetVideoAnalyticsAsync_WithValidVideoId_ReturnsAnalytics()
    {
        // Arrange
        var accessToken = "valid_token";
        var videoId = "video_123";

        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(@"{
                ""items"": [{
                    ""id"": ""video_123"",
                    ""statistics"": {
                        ""viewCount"": ""5000"",
                        ""likeCount"": ""500"",
                        ""commentCount"": ""50""
                    }
                }]
            }")
        };

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);

        var httpClient = new HttpClient(mockHandler.Object);
        var service = new YouTubeService(httpClient, _loggerMock.Object);

        // Act
        var result = await service.GetVideoAnalyticsAsync(accessToken, videoId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Views.Should().Be(5000);
        result.Data.Likes.Should().Be(500);
        result.Data.Comments.Should().Be(50);
    }

    [Fact]
    public async Task GetVideoAnalyticsAsync_WithEmptyVideoId_ReturnsFailResult()
    {
        // Act
        var result = await _service.GetVideoAnalyticsAsync("token", "");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_VIDEO_ID");
    }

    [Fact]
    public async Task CreatePlaylistAsync_WithValidData_ReturnsPlaylistId()
    {
        // Arrange
        var accessToken = "valid_token";
        var title = "My Playlist";
        var description = "Test playlist";

        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(@"{
                ""id"": ""playlist_123"",
                ""kind"": ""youtube#playlist""
            }")
        };

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);

        var httpClient = new HttpClient(mockHandler.Object);
        var service = new YouTubeService(httpClient, _loggerMock.Object);

        // Act
        var result = await service.CreatePlaylistAsync(accessToken, title, description);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().Be("playlist_123");
    }

    [Fact]
    public async Task CreatePlaylistAsync_WithEmptyTitle_ReturnsFailResult()
    {
        // Act
        var result = await _service.CreatePlaylistAsync("token", "", "description");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_TITLE");
    }

    [Fact]
    public async Task GetChannelInfoAsync_WithUnauthorizedResponse_ReturnsFailResult()
    {
        // Arrange
        var accessToken = "invalid_token";
        
        var mockResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent(@"{""error"": {""message"": ""Invalid token""}}")
        };

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);

        var httpClient = new HttpClient(mockHandler.Object);
        var service = new YouTubeService(httpClient, _loggerMock.Object);

        // Act
        var result = await service.GetChannelInfoAsync(accessToken);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_TOKEN");
    }

    [Fact]
    public async Task GetVideoAnalyticsAsync_WithNotFoundResponse_ReturnsFailResult()
    {
        // Arrange
        var accessToken = "valid_token";
        var videoId = "nonexistent_123";

        var mockResponse = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(@"{""error"": {""message"": ""Not found""}}")
        };

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);

        var httpClient = new HttpClient(mockHandler.Object);
        var service = new YouTubeService(httpClient, _loggerMock.Object);

        // Act
        var result = await service.GetVideoAnalyticsAsync(accessToken, videoId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("FETCH_FAILED");
    }
}
