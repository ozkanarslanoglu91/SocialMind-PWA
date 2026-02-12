using SocialMind.Shared.Models;
using SocialMind.Shared.Services;

namespace SocialMind.Web.Services;

/// <summary>
/// Mock AI servisi - test ve geliştirme için
/// </summary>
public class MockAIService : IAIService
{
    private readonly Random _random = new();

    private readonly string[] _sampleContents = new[]
    {
        "🚀 Yapay zeka ile sosyal medya yönetimi artık çok daha kolay! #AI #SocialMedia #Technology",
        "📊 Analitiklerinizi takip edin, stratejinizi geliştirin! #Analytics #Marketing #Growth",
        "💡 İçerik oluşturma sürecinizi hızlandırın, zamanınızı değerlendirin! #ContentCreation #Productivity",
        "🎯 Doğru zamanda, doğru platformda paylaşım yapın! #SmartScheduling #SocialStrategy",
        "✨ Tüm sosyal medya hesaplarınızı tek yerden yönetin! #SocialMind #Efficiency"
    };

    public Task<AIGeneratedContent> GenerateContentAsync(string prompt, string modelId, string language = "tr")
    {
        var content = new AIGeneratedContent
        {
            Content = _sampleContents[_random.Next(_sampleContents.Length)],
            Prompt = prompt,
            Provider = GetProviderFromModelId(modelId),
            ModelId = modelId,
            GeneratedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                { "language", language },
                { "tokensUsed", _random.Next(50, 200) }
            }
        };

        return Task.FromResult(content);
    }

    public async Task<List<AIGeneratedContent>> GenerateMultipleContentAsync(string prompt, string modelId, int count = 3)
    {
        var contents = new List<AIGeneratedContent>();

        for (int i = 0; i < count; i++)
        {
            var content = await GenerateContentAsync(prompt, modelId);
            content.Id = Guid.NewGuid().ToString();
            contents.Add(content);
        }

        return contents;
    }

    public Task<string> TranslateContentAsync(string content, string targetLanguage, string modelId)
    {
        // Basit mock çeviri
        var translations = new Dictionary<string, string>
        {
            { "en", "🚀 Social media management with AI is now much easier! #AI #SocialMedia #Technology" },
            { "tr", "🚀 Yapay zeka ile sosyal medya yönetimi artık çok daha kolay! #AI #SocialMedia #Technology" },
            { "es", "🚀 ¡La gestión de redes sociales con IA ahora es mucho más fácil! #AI #SocialMedia #Technology" },
            { "fr", "🚀 La gestion des médias sociaux avec l'IA est maintenant beaucoup plus facile! #AI #SocialMedia #Technology" }
        };

        var translated = translations.ContainsKey(targetLanguage)
            ? translations[targetLanguage]
            : content;

        return Task.FromResult(translated);
    }

    public Task<string> OptimizeContentAsync(string content, SocialPlatform platform, string modelId)
    {
        var optimized = content;
        var maxLength = PlatformRegistry.GetMaxCharacterLimit(platform);

        // Platform için optimize et
        if (content.Length > maxLength)
        {
            optimized = content.Substring(0, maxLength - 3) + "...";
        }

        // Platform özel optimizasyonlar
        switch (platform)
        {
            case SocialPlatform.Twitter:
                // Twitter için kısa ve öz
                if (!optimized.Contains("#"))
                {
                    optimized += " #AI #Tech";
                }
                break;
            case SocialPlatform.LinkedIn:
                // LinkedIn için profesyonel ton
                optimized = "💼 " + optimized;
                break;
            case SocialPlatform.Instagram:
                // Instagram için emoji ve hashtag
                if (!optimized.Contains("📸") && !optimized.Contains("🎨"))
                {
                    optimized = "📸 " + optimized;
                }
                break;
        }

        return Task.FromResult(optimized);
    }

    public Task<List<string>> GenerateHashtagsAsync(string content, SocialPlatform platform, int count = 10)
    {
        var hashtags = new List<string>
        {
            "ai", "technology", "socialmedia", "marketing", "digitalmarketing",
            "contentcreation", "business", "startup", "innovation", "growth",
            "analytics", "automation", "productivity", "strategy", "future"
        };

        // Platforma göre özelleştir
        if (platform == SocialPlatform.Instagram)
        {
            hashtags.AddRange(new[] { "insta", "instagood", "photooftheday", "picoftheday" });
        }
        else if (platform == SocialPlatform.Twitter)
        {
            hashtags.AddRange(new[] { "tech", "news", "trending" });
        }
        else if (platform == SocialPlatform.LinkedIn)
        {
            hashtags.AddRange(new[] { "professional", "career", "leadership", "businesstips" });
        }

        return Task.FromResult(hashtags.Take(count).ToList());
    }

    public Task<string> AnalyzeSentimentAsync(string content, string modelId)
    {
        // Basit sentiment analizi
        var positiveWords = new[] { "harika", "mükemmel", "güzel", "başarılı", "mutlu", "seviyorum", "amazing", "great", "good", "excellent" };
        var negativeWords = new[] { "kötü", "berbat", "üzgün", "başarısız", "sorun", "problem", "bad", "terrible", "sad", "fail" };

        var lowerContent = content.ToLower();
        var positiveCount = positiveWords.Count(word => lowerContent.Contains(word));
        var negativeCount = negativeWords.Count(word => lowerContent.Contains(word));

        if (positiveCount > negativeCount)
            return Task.FromResult("positive");
        if (negativeCount > positiveCount)
            return Task.FromResult("negative");

        return Task.FromResult("neutral");
    }

    public Task<bool> IsValidApiKeyAsync(string apiKey)
    {
        // Mock olarak her zaman true döndür
        return Task.FromResult(!string.IsNullOrEmpty(apiKey) && apiKey.Length > 10);
    }

    private AIProvider GetProviderFromModelId(string modelId)
    {
        if (modelId.Contains("gpt") && modelId.Contains("azure")) return AIProvider.AzureOpenAI;
        if (modelId.Contains("phi") || modelId.Contains("foundry")) return AIProvider.MicrosoftFoundry;
        if (modelId.Contains("gpt")) return AIProvider.OpenAI;
        if (modelId.Contains("gemini")) return AIProvider.GoogleGemini;
        if (modelId.Contains("llama") || modelId.Contains("mistral")) return AIProvider.NVIDIA;
        if (modelId.Contains("github")) return AIProvider.GitHubModels;

        return AIProvider.OpenAI;
    }
}
