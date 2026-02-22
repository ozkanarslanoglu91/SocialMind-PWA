namespace SocialMind.Shared.Models;

public class Post
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = string.Empty;
    public List<string> Platforms { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ScheduledFor { get; set; }
    public List<string> MediaUrls { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class SocialAccount
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Platform { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public int Followers { get; set; }
    public bool IsConnected { get; set; }
}

public class Analytics
{
    public string Id { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public int Impressions { get; set; }
    public int Engagements { get; set; }
    public int Likes { get; set; }
    public int Comments { get; set; }
    public int Shares { get; set; }
    public double EngagementRate { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
}

public class Campaign
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<string> PostIds { get; set; } = new();
    public string Status { get; set; } = "Draft";
}

public class AIGenerationRequest
{
    public string Topic { get; set; } = string.Empty;
    public string Tone { get; set; } = "Professional";
    public List<string> Platforms { get; set; } = new();
    public string? ImageUrl { get; set; }
}

public class AIGenerationResponse
{
    public List<string> Suggestions { get; set; } = new();
    public List<string> Hashtags { get; set; } = new();
    public string ImageDescription { get; set; } = string.Empty;
}
