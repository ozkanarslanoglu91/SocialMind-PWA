using SocialMind.Shared.Models;
using SocialMind.Shared.Services;

namespace SocialMind.Web.Services;

/// <summary>
/// Mock Schedule servisi - test ve geliştirme için
/// </summary>
public class MockScheduleService : IScheduleService
{
    private readonly List<ScheduleConfiguration> _schedules = new();
    private readonly Random _random = new();

    public Task<ScheduleConfiguration> CreateScheduleAsync(ScheduleConfiguration schedule)
    {
        schedule.Id = Guid.NewGuid().ToString();
        _schedules.Add(schedule);
        return Task.FromResult(schedule);
    }

    public Task<ScheduleConfiguration> UpdateScheduleAsync(ScheduleConfiguration schedule)
    {
        var existing = _schedules.FirstOrDefault(s => s.Id == schedule.Id);
        if (existing != null)
        {
            var index = _schedules.IndexOf(existing);
            _schedules[index] = schedule;
        }
        return Task.FromResult(schedule);
    }

    public Task<bool> DeleteScheduleAsync(string scheduleId)
    {
        var schedule = _schedules.FirstOrDefault(s => s.Id == scheduleId);
        if (schedule != null)
        {
            _schedules.Remove(schedule);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<ScheduleConfiguration?> GetScheduleAsync(string scheduleId)
    {
        var schedule = _schedules.FirstOrDefault(s => s.Id == scheduleId);
        return Task.FromResult(schedule);
    }

    public Task<List<ScheduleConfiguration>> GetSchedulesForPostAsync(string postId)
    {
        var schedules = _schedules.Where(s => s.PostId == postId).ToList();
        return Task.FromResult(schedules);
    }

    public Task<List<ScheduleConfiguration>> GetAllSchedulesAsync()
    {
        return Task.FromResult(_schedules.ToList());
    }

    public Task<SmartScheduleSuggestion> GetOptimalScheduleSuggestionAsync(string postId, SocialPlatform platform)
    {
        // Akıllı zamanlama önerisi
        var bestDays = new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday };
        var bestHours = new[] { 9, 12, 15, 18, 20 }; // En iyi saatler

        var suggestion = new SmartScheduleSuggestion
        {
            Platform = platform,
            SuggestedTime = GetNextBestTime(bestDays, bestHours),
            ConfidenceScore = (decimal)(_random.NextDouble() * 0.3 + 0.7), // 0.7-1.0 arası
            Reason = GetReasonForPlatform(platform),
            PredictedEngagement = _random.Next(500, 5000),
            SimilarTopPosts = new List<string> { "post_1", "post_2", "post_3" }
        };

        return Task.FromResult(suggestion);
    }

    public async Task<List<SmartScheduleSuggestion>> GetOptimalScheduleSuggestionsAsync(string postId)
    {
        var platforms = Enum.GetValues<SocialPlatform>();
        var suggestions = new List<SmartScheduleSuggestion>();

        foreach (var platform in platforms)
        {
            suggestions.Add(await GetOptimalScheduleSuggestionAsync(postId, platform));
        }

        return suggestions;
    }

    public Task<PostCalendar> GetCalendarForMonthAsync(int year, int month)
    {
        var calendar = new PostCalendar
        {
            Date = new DateTime(year, month, 1),
            Posts = new List<PostCalendarItem>()
        };

        // Örnek planlanmış postlar ekle
        var daysInMonth = DateTime.DaysInMonth(year, month);
        for (int day = 1; day <= daysInMonth; day += 3)
        {
            calendar.Posts.Add(new PostCalendarItem
            {
                PostId = Guid.NewGuid().ToString(),
                Title = $"Planlanmış Post {day}",
                ScheduledTime = new DateTime(year, month, day, 10, 0, 0),
                Platform = (SocialPlatform)(day % 6),
                Status = "Scheduled"
            });
        }

        return Task.FromResult(calendar);
    }

    public Task<List<PostCalendarItem>> GetUpcomingPostsAsync(int daysAhead = 7)
    {
        var posts = new List<PostCalendarItem>();
        var now = DateTime.UtcNow;

        for (int i = 1; i <= daysAhead; i++)
        {
            posts.Add(new PostCalendarItem
            {
                PostId = Guid.NewGuid().ToString(),
                Title = $"Yaklaşan Post - {i}",
                ScheduledTime = now.AddDays(i).AddHours(10),
                Platform = (SocialPlatform)(i % 6),
                Status = "Scheduled"
            });
        }

        return Task.FromResult(posts);
    }

    public Task<List<PostCalendarItem>> GetPostsForDateAsync(DateTime date)
    {
        var posts = new List<PostCalendarItem>();

        // O gün için örnek postlar
        for (int i = 0; i < 3; i++)
        {
            posts.Add(new PostCalendarItem
            {
                PostId = Guid.NewGuid().ToString(),
                Title = $"Post {i + 1} - {date:dd MMM}",
                ScheduledTime = date.AddHours(9 + i * 3),
                Platform = (SocialPlatform)(i % 6),
                Status = "Scheduled"
            });
        }

        return Task.FromResult(posts);
    }

    public Task<bool> ExecuteScheduledPostsAsync()
    {
        // Zamanı gelen postları işle
        var now = DateTime.UtcNow;
        var dueSchedules = _schedules.Where(s =>
            s.IsActive &&
            s.ScheduledTime.HasValue &&
            s.ScheduledTime.Value <= now).ToList();

        foreach (var schedule in dueSchedules)
        {
            schedule.IsActive = false;
            // Burada gerçekte post yayımlanır
        }

        return Task.FromResult(true);
    }

    private DateTime GetNextBestTime(DayOfWeek[] bestDays, int[] bestHours)
    {
        var now = DateTime.UtcNow;
        var currentDay = now.DayOfWeek;

        // En yakın iyi günü bul
        var nextBestDay = bestDays.FirstOrDefault(d => d > currentDay);
        if (nextBestDay == 0) // Bu hafta bulunamadı, gelecek haftayı kullan
        {
            nextBestDay = bestDays[0];
        }

        int daysToAdd = ((int)nextBestDay - (int)currentDay + 7) % 7;
        if (daysToAdd == 0) daysToAdd = 7;

        var nextDate = now.AddDays(daysToAdd);
        var bestHour = bestHours[_random.Next(bestHours.Length)];

        return new DateTime(nextDate.Year, nextDate.Month, nextDate.Day, bestHour, 0, 0);
    }

    private string GetReasonForPlatform(SocialPlatform platform)
    {
        return platform switch
        {
            SocialPlatform.Twitter => "Twitter'da en yüksek etkileşim sabah 9-11 ve akşam 18-20 arası",
            SocialPlatform.LinkedIn => "LinkedIn'de iş günlerinde sabah 8-10 arası en aktif dönem",
            SocialPlatform.Instagram => "Instagram'da öğle 12-14 ve akşam 19-21 arası en iyi sonuçlar",
            SocialPlatform.Facebook => "Facebook'ta hafta içi öğlen ve akşam saatleri ideal",
            SocialPlatform.TikTok => "TikTok'ta gece 21-23 arası en fazla kullanıcı aktif",
            SocialPlatform.YouTube => "YouTube'da akşam 18-22 arası video izlenme oranı en yüksek",
            _ => "Analiz verilerine göre optimal zaman"
        };
    }
}
