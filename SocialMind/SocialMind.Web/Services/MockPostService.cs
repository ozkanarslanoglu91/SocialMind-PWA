using SocialMind.Shared.Models;
using SocialMind.Shared.Services;

namespace SocialMind.Web.Services;

/// <summary>
/// Mock Post servisi - test ve geliştirme için
/// </summary>
public class MockPostService : IPostService
{
    private readonly List<Post> _posts = new();
    private readonly Dictionary<string, List<PostMedia>> _postMedia = new();

    public MockPostService()
    {
        // Örnek veri ekle
        InitializeSampleData();
    }

    private void InitializeSampleData()
    {
        var samplePost = new Post
        {
            Id = "1",
            Title = "İlk Gönderim",
            Content = "SocialMind ile sosyal medya yönetimi artık çok kolay! 🚀 #SocialMedia #AI",
            Platforms = new List<SocialPlatform> { SocialPlatform.Twitter, SocialPlatform.LinkedIn },
            Status = PostStatus.Published,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            PublishedAt = DateTime.UtcNow.AddDays(-2),
            Hashtags = new List<string> { "SocialMedia", "AI", "Technology" }
        };

        _posts.Add(samplePost);
    }

    public Task<Post> CreatePostAsync(Post post)
    {
        post.Id = Guid.NewGuid().ToString();
        post.CreatedAt = DateTime.UtcNow;
        _posts.Add(post);
        return Task.FromResult(post);
    }

    public Task<Post> UpdatePostAsync(Post post)
    {
        var existingPost = _posts.FirstOrDefault(p => p.Id == post.Id);
        if (existingPost != null)
        {
            var index = _posts.IndexOf(existingPost);
            _posts[index] = post;
        }
        return Task.FromResult(post);
    }

    public Task<bool> DeletePostAsync(string postId)
    {
        var post = _posts.FirstOrDefault(p => p.Id == postId);
        if (post != null)
        {
            _posts.Remove(post);
            _postMedia.Remove(postId);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<Post?> GetPostAsync(string postId)
    {
        var post = _posts.FirstOrDefault(p => p.Id == postId);
        return Task.FromResult(post);
    }

    public Task<List<Post>> GetAllPostsAsync(PostStatus? status = null)
    {
        var query = _posts.AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }
        return Task.FromResult(query.OrderByDescending(p => p.CreatedAt).ToList());
    }

    public Task<List<Post>> GetPostsByPlatformAsync(SocialPlatform platform)
    {
        var posts = _posts.Where(p => p.Platforms.Contains(platform))
                         .OrderByDescending(p => p.CreatedAt)
                         .ToList();
        return Task.FromResult(posts);
    }

    public Task<List<Post>> GetScheduledPostsAsync()
    {
        var posts = _posts.Where(p => p.Status == PostStatus.Scheduled)
                         .OrderBy(p => p.ScheduledAt)
                         .ToList();
        return Task.FromResult(posts);
    }

    public Task<PostMedia> AddMediaAsync(string postId, PostMedia media)
    {
        if (!_postMedia.ContainsKey(postId))
        {
            _postMedia[postId] = new List<PostMedia>();
        }

        media.Id = Guid.NewGuid().ToString();
        media.UploadedAt = DateTime.UtcNow;
        _postMedia[postId].Add(media);

        return Task.FromResult(media);
    }

    public Task<bool> RemoveMediaAsync(string postId, string mediaId)
    {
        if (_postMedia.TryGetValue(postId, out var mediaList))
        {
            var media = mediaList.FirstOrDefault(m => m.Id == mediaId);
            if (media != null)
            {
                mediaList.Remove(media);
                return Task.FromResult(true);
            }
        }
        return Task.FromResult(false);
    }

    public Task<List<PostMedia>> GetPostMediaAsync(string postId)
    {
        if (_postMedia.TryGetValue(postId, out var mediaList))
        {
            return Task.FromResult(mediaList);
        }
        return Task.FromResult(new List<PostMedia>());
    }

    public Task<PublishedPostResult> PublishPostAsync(string postId)
    {
        var post = _posts.FirstOrDefault(p => p.Id == postId);
        if (post != null)
        {
            post.Status = PostStatus.Published;
            post.PublishedAt = DateTime.UtcNow;

            return Task.FromResult(new PublishedPostResult
            {
                PostId = postId,
                Platform = post.Platforms.FirstOrDefault(),
                PlatformPostId = Guid.NewGuid().ToString(),
                Url = "https://example.com/post/123",
                PublishedAt = DateTime.UtcNow,
                IsSuccess = true
            });
        }

        return Task.FromResult(new PublishedPostResult
        {
            PostId = postId,
            IsSuccess = false,
            ErrorMessage = "Post bulunamadı"
        });
    }

    public Task<List<PublishedPostResult>> PublishToMultiplePlatformsAsync(string postId)
    {
        var post = _posts.FirstOrDefault(p => p.Id == postId);
        if (post != null)
        {
            post.Status = PostStatus.Published;
            post.PublishedAt = DateTime.UtcNow;

            var results = post.Platforms.Select(platform => new PublishedPostResult
            {
                PostId = postId,
                Platform = platform,
                PlatformPostId = Guid.NewGuid().ToString(),
                Url = $"https://example.com/{platform.ToString().ToLower()}/post/123",
                PublishedAt = DateTime.UtcNow,
                IsSuccess = true
            }).ToList();

            return Task.FromResult(results);
        }

        return Task.FromResult(new List<PublishedPostResult>());
    }

    public Task<bool> ReschedulePostAsync(string postId, DateTime newTime)
    {
        var post = _posts.FirstOrDefault(p => p.Id == postId);
        if (post != null)
        {
            post.ScheduledAt = newTime;
            post.Status = PostStatus.Scheduled;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}
