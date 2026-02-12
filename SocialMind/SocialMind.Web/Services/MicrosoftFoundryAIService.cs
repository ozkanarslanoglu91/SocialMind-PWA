using System.Text;
using System.Text.Json;
using SocialMind.Shared.Models;
using SocialMind.Shared.Services;

namespace SocialMind.Web.Services;

/// <summary>
/// Microsoft Foundry (Azure AI) servisi - Gerçek API entegrasyonu
/// Azure AI Inference API kullanır
/// </summary>
public class MicrosoftFoundryAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _endpoint;
    private readonly ILogger<MicrosoftFoundryAIService> _logger;

    public MicrosoftFoundryAIService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<MicrosoftFoundryAIService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("MicrosoftFoundry");
        _apiKey = configuration["AIProviders:MicrosoftFoundry:ApiKey"] ?? string.Empty;
        _endpoint = configuration["AIProviders:MicrosoftFoundry:BaseUrl"] ??
                    "https://models.inference.ai.azure.com";
        _logger = logger;
    }

    public async Task<AIGeneratedContent> GenerateContentAsync(string prompt, string modelId, string language = "tr")
    {
        try
        {
            var systemPrompt = GetSystemPrompt(language);
            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = 500,
                top_p = 0.95
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"/chat/completions")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json")
            };

            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Headers.Add("api-version", "2024-05-01");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AzureInferenceResponse>(responseContent);

            var generatedText = result?.Choices?.FirstOrDefault()?.Message?.Content ??
                               "İçerik oluşturulamadı.";

            return new AIGeneratedContent
            {
                Content = generatedText,
                Prompt = prompt,
                Provider = AIProvider.MicrosoftFoundry,
                ModelId = modelId,
                GeneratedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    { "language", language },
                    { "tokensUsed", result?.Usage?.TotalTokens ?? 0 },
                    { "model", result?.Model ?? modelId }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Microsoft Foundry AI servisi hatası: {Message}", ex.Message);

            // Fallback olarak basit bir içerik döndür
            return new AIGeneratedContent
            {
                Content = GenerateFallbackContent(prompt, language),
                Prompt = prompt,
                Provider = AIProvider.MicrosoftFoundry,
                ModelId = modelId,
                GeneratedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    { "error", ex.Message },
                    { "fallback", true }
                }
            };
        }
    }

    public async Task<List<AIGeneratedContent>> GenerateMultipleContentAsync(
        string prompt,
        string modelId,
        int count = 3)
    {
        var contents = new List<AIGeneratedContent>();
        var tasks = new List<Task<AIGeneratedContent>>();

        // Paralel olarak birden fazla içerik üret
        for (int i = 0; i < count; i++)
        {
            var modifiedPrompt = $"{prompt} (Varyasyon {i + 1}: Farklı bir yaklaşım kullan)";
            tasks.Add(GenerateContentAsync(modifiedPrompt, modelId));
        }

        var results = await Task.WhenAll(tasks);
        return results.ToList();
    }

    public async Task<string> TranslateContentAsync(string content, string targetLanguage, string modelId)
    {
        var prompt = $"Aşağıdaki metni {GetLanguageName(targetLanguage)} diline çevir. " +
                     $"Sadece çeviriyi döndür, açıklama ekleme:\n\n{content}";

        var result = await GenerateContentAsync(prompt, modelId, targetLanguage);
        return result.Content;
    }

    public async Task<string> OptimizeContentAsync(string content, SocialPlatform platform, string modelId)
    {
        var maxLength = PlatformRegistry.GetMaxCharacterLimit(platform);
        var platformName = platform.ToString();

        var prompt = $"Bu sosyal medya içeriğini {platformName} platformu için optimize et. " +
                     $"Maksimum {maxLength} karakter olmalı. Emojiler ve hashtagler kullan. " +
                     $"Platform kurallarına uygun hale getir:\n\n{content}";

        var result = await GenerateContentAsync(prompt, modelId);

        // Karakter limitini aşıyorsa kısalt
        if (result.Content.Length > maxLength)
        {
            return result.Content.Substring(0, maxLength - 3) + "...";
        }

        return result.Content;
    }

    public async Task<List<string>> GenerateHashtagsAsync(
        string content,
        SocialPlatform platform,
        int count = 10)
    {
        var prompt = $"{platform} platformu için bu içeriğe uygun {count} adet hashtag öner. " +
                     $"Sadece hashtag'leri virgülle ayırarak listele, # işareti kullanma:\n\n{content}";

        var result = await GenerateContentAsync(prompt, "phi-4");

        // Sonucu parse et
        var hashtags = result.Content
            .Split(new[] { ',', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(h => h.Trim().TrimStart('#'))
            .Where(h => !string.IsNullOrWhiteSpace(h))
            .Take(count)
            .ToList();

        return hashtags;
    }

    public async Task<string> AnalyzeSentimentAsync(string content, string modelId)
    {
        var prompt = $"Bu içeriğin duygusal tonunu analiz et. " +
                     $"Sadece şu kelimelerden birini döndür: positive, negative, neutral\n\n{content}";

        var result = await GenerateContentAsync(prompt, modelId);

        var sentiment = result.Content.ToLower().Trim();
        if (sentiment.Contains("positive")) return "positive";
        if (sentiment.Contains("negative")) return "negative";
        return "neutral";
    }

    public async Task<bool> IsValidApiKeyAsync(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey.Length < 20)
            return false;

        try
        {
            var testClient = new HttpClient { BaseAddress = new Uri(_endpoint) };
            var request = new HttpRequestMessage(HttpMethod.Get, "/models");
            request.Headers.Add("Authorization", $"Bearer {apiKey}");

            var response = await testClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // Helper metodlar
    private string GetSystemPrompt(string language)
    {
        return language switch
        {
            "tr" => "Sen profesyonel bir sosyal medya içerik yazarısın. " +
                    "Türkçe dilinde, ilgi çekici, kısa ve etkili içerikler üretiyorsun. " +
                    "Emoji kullanmayı seviyorsun ve hashtag önerilerinde bulunuyorsun.",

            "en" => "You are a professional social media content writer. " +
                    "You create engaging, concise, and effective content in English. " +
                    "You use emojis and suggest relevant hashtags.",

            _ => "You are a professional social media content writer. " +
                 "Create engaging and effective content with emojis and hashtags."
        };
    }

    private string GetLanguageName(string languageCode)
    {
        return languageCode switch
        {
            "tr" => "Türkçe",
            "en" => "İngilizce",
            "es" => "İspanyolca",
            "fr" => "Fransızca",
            "de" => "Almanca",
            "it" => "İtalyanca",
            "pt" => "Portekizce",
            _ => "İngilizce"
        };
    }

    private string GenerateFallbackContent(string prompt, string language)
    {
        var templates = new[]
        {
            "🚀 {topic} ile ilgili harika bir içerik! #Innovation #Tech",
            "💡 {topic} hakkında düşünmeniz gereken 3 şey... #Tips #SocialMedia",
            "✨ {topic} dünyasında yenilikler! #Future #Digital",
            "📊 {topic} stratejinizi geliştirin! #Growth #Success",
            "🎯 {topic} ile başarıya ulaşın! #Goals #Achievement"
        };

        var random = new Random();
        var template = templates[random.Next(templates.Length)];
        var topic = prompt.Length > 30 ? prompt.Substring(0, 30) + "..." : prompt;

        return template.Replace("{topic}", topic);
    }

    // Azure AI Inference API Response modelleri
    private class AzureInferenceResponse
    {
        public string? Model { get; set; }
        public Choice[]? Choices { get; set; }
        public Usage? Usage { get; set; }
    }

    private class Choice
    {
        public int Index { get; set; }
        public Message? Message { get; set; }
        public string? FinishReason { get; set; }
    }

    private class Message
    {
        public string? Role { get; set; }
        public string? Content { get; set; }
    }

    private class Usage
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }
}
