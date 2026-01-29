namespace SocialMind.Shared.Models;

/// <summary>
/// Post durumu
/// </summary>
public enum PostStatus
{
    Draft = 0,
    Scheduled = 1,
    Published = 2,
    Failed = 3,
    Archived = 4
}

/// <summary>
/// Sosyal medya gönderisi
/// </summary>
public class Post
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<SocialPlatform> Platforms { get; set; } = [];
    public PostStatus Status { get; set; } = PostStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ScheduledAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public List<PostMedia> MediaItems { get; set; } = [];
    public List<string>? Hashtags { get; set; }
    public string? AIModelUsed { get; set; }
    public string? AIPrompt { get; set; }
    public Dictionary<string, PostPlatformData>? PlatformData { get; set; }
    public bool IsPinned { get; set; }
    public bool IsFavorite { get; set; }
}

/// <summary>
/// Yayınlanan post sonuçları
/// </summary>
public class PublishedPostResult
{
    public string PostId { get; set; } = string.Empty;
    public SocialPlatform Platform { get; set; }
    public string PlatformPostId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Post medya dosyası
/// </summary>
public class PostMedia
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FileName { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty; // image/jpeg, video/mp4 etc
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? ThumbnailPath { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string? AltText { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Platform özel post verisi
/// </summary>
public class PostPlatformData
{
    public SocialPlatform Platform { get; set; }
    public string ContentForPlatform { get; set; } = string.Empty;
    public List<string>? PlatformSpecificHashtags { get; set; }
    public bool IncludeLink { get; set; }
    public string? CustomUrl { get; set; }
    public Dictionary<string, object>? PlatformSpecificSettings { get; set; }
}

/// <summary>
/// Post önizlemesi
/// </summary>
public class PostPreview
{
    public SocialPlatform Platform { get; set; }
    public string PreviewHtml { get; set; } = string.Empty;
    public string DisplayContent { get; set; } = string.Empty;
    public int CharacterCount { get; set; }
    public int MaxCharacters { get; set; }
    public List<string> Warnings { get; set; } = [];
    public List<string> Suggestions { get; set; } = [];
}
