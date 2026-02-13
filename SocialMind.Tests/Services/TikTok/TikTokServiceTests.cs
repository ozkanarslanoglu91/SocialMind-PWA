using System.Net;
using FluentAssertions;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using SocialMind.Shared.Models;
using SocialMind.Web.Services;

namespace SocialMind.Tests.Services.TikTok;

public class TikTokServiceTests
{
    private readonly Mock<ILogger<TikTokService>> _loggerMock;
    private readonly HttpClient _httpClient;

    public TikTokServiceTests()
    {
        _loggerMock = new Mock<ILogger<TikTokService>>();
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://open.tiktok.com/v1")
        };
    }

    [Fact]
    public async Task UploadVideoAsync_WithValidToken_ReturnsSuccessResult()
    {
        // Arrange
        var accessToken = "valid_token";
        var videoPath = "test_video.mp4";
        var post = new Post { Caption = "Test TikTok video", Content = "Test content" };

        File.WriteAllBytes(videoPath, new byte[] { 1, 2, 3, 4, 5 });

        try
        {
            var initResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""data"": {
                        ""upload_id"": ""test_upload_id_123""
                    }
                }")
            };

            var publishResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""data"": {
                        ""video_id"": ""tiktok_123""
                    }
                }")
            };

            var mockHandler = new Mock<HttpMessageHandler>();
            var callCount = 0;

            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Returns((HttpRequestMessage request, CancellationToken ct) =>
                {
                    callCount++;
                    // First call: init, last call: publish, middle calls: chunks
                    if (callCount == 1)
                        return Task.FromResult(initResponse);
                    else if (request.Method == HttpMethod.Post && request.RequestUri!.ToString().Contains("publish"))
                        return Task.FromResult(publishResponse);
                    else
                        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var service = new TikTokService(httpClient, _loggerMock.Object);

            // Act
            var result = await service.UploadVideoAsync(accessToken, videoPath, post);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.ExternalId.Should().Be("tiktok_123");
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
            var service = new TikTokService(_httpClient, _loggerMock.Object);

            // Act
            var result = await service.UploadVideoAsync("", videoPath, post);

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
        var service = new TikTokService(_httpClient, _loggerMock.Object);
        var post = new Post { Caption = "Test" };

        // Act
        var result = await service.UploadVideoAsync("token", "nonexistent.mp4", post);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("FILE_NOT_FOUND");
    }

    [Fact]
    public async Task GetCreatorInfoAsync_WithValidToken_ReturnsCreatorInfo()
    {
        // Arrange
        var accessToken = "valid_token";

        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(@"{
                ""data"": {
                    ""user"": {
                        ""open_id"": ""creator_123"",
                        ""display_name"": ""Test Creator"",
                        ""avatar_large_url"": ""https://example.com/avatar.jpg"",
                        ""follower_count"": 10000,
                        ""following_count"": 500,
                        ""video_count"": 50,
                        ""like_count"": 100000
                    }
                }
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
        var service = new TikTokService(httpClient, _loggerMock.Object);

        // Act
        var result = await service.GetCreatorInfoAsync(accessToken);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Username.Should().Be("Test Creator");
        result.Data.FollowerCount.Should().Be(10000);
        result.Data.VideoCount.Should().Be(50);
        result.Data.LikeCount.Should().Be(100000);
    }

    [Fact]
    public async Task GetCreatorInfoAsync_WithEmptyToken_ReturnsFailResult()
    {
        // Arrange
        var service = new TikTokService(_httpClient, _loggerMock.Object);

        // Act
        var result = await service.GetCreatorInfoAsync("");

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
                ""data"": {
                    ""videos"": [{
                        ""id"": ""video_123"",
                        ""play_count"": 5000,
                        ""like_count"": 500,
                        ""comment_count"": 50,
                        ""share_count"": 25
                    }]
                }
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
        var service = new TikTokService(httpClient, _loggerMock.Object);

        // Act
        var result = await service.GetVideoAnalyticsAsync(accessToken, videoId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Views.Should().Be(5000);
        result.Data.Likes.Should().Be(500);
        result.Data.Comments.Should().Be(50);
        result.Data.Shares.Should().Be(25);
    }

    [Fact]
    public async Task GetVideoAnalyticsAsync_WithEmptyVideoId_ReturnsFailResult()
    {
        // Arrange
        var service = new TikTokService(_httpClient, _loggerMock.Object);

        // Act
        var result = await service.GetVideoAnalyticsAsync("token", "");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_VIDEO_ID");
    }

    [Fact]
    public async Task ScheduleVideoAsync_WithFutureDate_ReturnsSuccessResult()
    {
        // Arrange
        var accessToken = "valid_token";
        var videoPath = "test_video.mp4";
        var post = new Post { Caption = "Scheduled video" };
        var futureTime = DateTime.UtcNow.AddHours(1);

        File.WriteAllBytes(videoPath, new byte[] { 1, 2, 3, 4, 5 });

        try
        {
            var initResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""data"": {
                        ""upload_id"": ""schedule_upload_id""
                    }
                }")
            };

            var scheduleResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""data"": {
                        ""video_id"": ""scheduled_video_123""
                    }
                }")
            };

            var mockHandler = new Mock<HttpMessageHandler>();
            var callCount = 0;

            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Returns((HttpRequestMessage request, CancellationToken ct) =>
                {
                    callCount++;
                    if (callCount == 1)
                        return Task.FromResult(initResponse);
                    else if (request.Method == HttpMethod.Post && request.RequestUri!.ToString().Contains("publish"))
                        return Task.FromResult(scheduleResponse);
                    else
                        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var service = new TikTokService(httpClient, _loggerMock.Object);

            // Act
            var result = await service.ScheduleVideoAsync(accessToken, videoPath, post, futureTime);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().Be("scheduled_video_123");
        }
        finally
        {
            if (File.Exists(videoPath))
                File.Delete(videoPath);
        }
    }

    [Fact]
    public async Task ScheduleVideoAsync_WithPastDate_ReturnsFailResult()
    {
        // Arrange
        var accessToken = "valid_token";
        var videoPath = "test.mp4";
        var post = new Post { Caption = "Test" };
        var pastTime = DateTime.UtcNow.AddHours(-1);

        File.WriteAllBytes(videoPath, new byte[] { 1, 2, 3 });

        try
        {
            var service = new TikTokService(_httpClient, _loggerMock.Object);

            // Act
            var result = await service.ScheduleVideoAsync(accessToken, videoPath, post, pastTime);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be("INVALID_SCHEDULE_TIME");
        }
        finally
        {
            if (File.Exists(videoPath))
                File.Delete(videoPath);
        }
    }

    [Fact]
    public async Task GetCreatorInfoAsync_WithUnauthorizedResponse_ReturnsFailResult()
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
        var service = new TikTokService(httpClient, _loggerMock.Object);

        // Act
        var result = await service.GetCreatorInfoAsync(accessToken);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("FETCH_FAILED");
    }

    [Fact]
    public async Task UploadVideoAsync_WithNetworkError_ReturnsFailResult()
    {
        // Arrange
        var videoPath = "test.mp4";
        File.WriteAllBytes(videoPath, new byte[] { 1, 2, 3 });

        try
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            var httpClient = new HttpClient(mockHandler.Object);
            var service = new TikTokService(httpClient, _loggerMock.Object);

            // Act
            var result = await service.UploadVideoAsync("token", videoPath, new Post { Caption = "Test" });

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be("NETWORK_ERROR");
        }
        finally
        {
            if (File.Exists(videoPath))
                File.Delete(videoPath);
        }
    }
}
