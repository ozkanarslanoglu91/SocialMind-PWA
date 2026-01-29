namespace SocialMind.Shared.Models;

/// <summary>
/// Zamanlama türü
/// </summary>
public enum SchedulingType
{
    Immediate,
    SpecificTime,
    OptimalTime,
    Recurring
}

/// <summary>
/// Zamanlama yapılandırması
/// </summary>
public class ScheduleConfiguration
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PostId { get; set; } = string.Empty;
    public SchedulingType Type { get; set; } = SchedulingType.SpecificTime;
    public DateTime? ScheduledTime { get; set; }
    public RecurrencePattern? RecurrencePattern { get; set; }
    public bool UseOptimalTime { get; set; }
    public Dictionary<SocialPlatform, DateTime?> PlatformSpecificTimes { get; set; } = [];
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Tekrarlama deseni
/// </summary>
public class RecurrencePattern
{
    public RecurrenceType Type { get; set; }
    public int Interval { get; set; } = 1; // 1, 2, 3 etc
    public DayOfWeek[] DaysOfWeek { get; set; } = [];
    public int? DayOfMonth { get; set; }
    public DateTime? EndDate { get; set; }
    public int? MaxOccurrences { get; set; }
}

/// <summary>
/// Tekrarlama türü
/// </summary>
public enum RecurrenceType
{
    Daily,
    Weekly,
    Monthly,
    Yearly
}

/// <summary>
/// Akıllı zamanlama önerisi
/// </summary>
public class SmartScheduleSuggestion
{
    public SocialPlatform Platform { get; set; }
    public DateTime SuggestedTime { get; set; }
    public decimal ConfidenceScore { get; set; } // 0-1
    public string Reason { get; set; } = string.Empty;
    public int PredictedEngagement { get; set; }
    public List<string> SimilarTopPosts { get; set; } = [];
}

/// <summary>
/// Geçiş zamanı takvimi
/// </summary>
public class PostCalendar
{
    public DateTime Date { get; set; }
    public List<PostCalendarItem> Posts { get; set; } = [];
}

/// <summary>
/// Post takvim öğesi
/// </summary>
public class PostCalendarItem
{
    public string PostId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime ScheduledTime { get; set; }
    public SocialPlatform Platform { get; set; }
    public string Status { get; set; } = string.Empty;
}
