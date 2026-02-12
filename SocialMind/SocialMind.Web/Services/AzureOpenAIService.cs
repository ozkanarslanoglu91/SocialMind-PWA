using System.Text;
using System.Text.Json;
using SocialMind.Shared.Models;
using SocialMind.Shared.Services;

namespace SocialMind.Web.Services;

/// <summary>
/// Azure OpenAI Service - Enterprise özellikleriyle OpenAI modelleri
/// Private endpoints, compliance, data residency destekler
/// </summary>
public class AzureOpenAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _endpoint;
    private readonly string _deploymentName;
    private readonly ILogger<AzureOpenAIService> _logger;

    public AzureOpenAIService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<AzureOpenAIService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("AzureOpenAI");
        _apiKey = configuration["AIProviders:AzureOpenAI:ApiKey"] ?? string.Empty;
        _endpoint = configuration["AIProviders:AzureOpenAI:Endpoint"] ?? string.Empty;
        _deploymentName = configuration["AIProviders:AzureOpenAI:DeploymentName"] ?? "gpt-4o";
        _logger = logger;

        if (!string.IsNullOrEmpty(_endpoint))
        {
            _httpClient.BaseAddress = new Uri(_endpoint);
        }
    }

    public async Task<AIGeneratedContent> GenerateContentAsync(string prompt, string modelId, string language = "tr")
    {
        try
        {
            var systemPrompt = $"Sen profesyonel bir sosyal medya içerik yazarısın. " +
                              $"{GetLanguageName(language)} dilinde içerik üretiyorsun.";

            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = 500,
                top_p = 0.95,
                frequency_penalty = 0.5,
                presence_penalty = 0.5
            };

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/openai/deployments/{_deploymentName}/chat/completions?api-version=2024-02-15-preview")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json")
            };

            request.Headers.Add("api-key", _apiKey);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AzureOpenAIResponse>(responseContent);

            var generatedText = result?.Choices?.FirstOrDefault()?.Message?.Content ??
                               "İçerik oluşturulamadı.";

            return new AIGeneratedContent
            {
                Content = generatedText,
                Prompt = prompt,
                Provider = AIProvider.AzureOpenAI,
                ModelId = modelId,
                GeneratedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    { "language", language },
                    { "tokensUsed", result?.Usage?.TotalTokens ?? 0 },
                    { "deployment", _deploymentName },
                    { "endpoint", _endpoint }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure OpenAI servisi hatası: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<List<AIGeneratedContent>> GenerateMultipleContentAsync(
        string prompt,
        string modelId,
        int count = 3)
    {
        var contents = new List<AIGeneratedContent>();

        for (int i = 0; i < count; i++)
        {
            var modifiedPrompt = $"{prompt}\n\nVaryasyon {i + 1}: Farklı bir ton ve yaklaşım kullan.";
            var content = await GenerateContentAsync(modifiedPrompt, modelId);
            content.Id = Guid.NewGuid().ToString();
            contents.Add(content);

            // Rate limiting için küçük bir gecikme
            if (i < count - 1)
            {
                await Task.Delay(500);
            }
        }

        return contents;
    }

    public async Task<string> TranslateContentAsync(string content, string targetLanguage, string modelId)
    {
        var prompt = $"Translate the following text to {GetLanguageName(targetLanguage)}. " +
                     $"Return only the translation:\n\n{content}";

        var result = await GenerateContentAsync(prompt, modelId, targetLanguage);
        return result.Content;
    }

    public async Task<string> OptimizeContentAsync(string content, SocialPlatform platform, string modelId)
    {
        var maxLength = PlatformRegistry.GetMaxCharacterLimit(platform);

        var prompt = $"Optimize this social media post for {platform}. " +
                     $"Maximum {maxLength} characters. Use emojis and hashtags appropriately:\n\n{content}";

        var result = await GenerateContentAsync(prompt, modelId);

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
        var prompt = $"Generate {count} relevant hashtags for this {platform} post. " +
                     $"Return only hashtags separated by commas, without # symbol:\n\n{content}";

        var result = await GenerateContentAsync(prompt, "gpt-35-turbo-azure");

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
        var prompt = $"Analyze the sentiment of this text. " +
                     $"Return only one word: positive, negative, or neutral\n\n{content}";

        var result = await GenerateContentAsync(prompt, modelId);

        var sentiment = result.Content.ToLower().Trim();
        if (sentiment.Contains("positive")) return "positive";
        if (sentiment.Contains("negative")) return "negative";
        return "neutral";
    }

    public async Task<bool> IsValidApiKeyAsync(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
            return false;

        try
        {
            var testClient = new HttpClient { BaseAddress = new Uri(_endpoint) };
            var request = new HttpRequestMessage(HttpMethod.Get, "/openai/deployments?api-version=2024-02-15-preview");
            request.Headers.Add("api-key", apiKey);

            var response = await testClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private string GetLanguageName(string languageCode)
    {
        return languageCode switch
        {
            "tr" => "Turkish",
            "en" => "English",
            "es" => "Spanish",
            "fr" => "French",
            "de" => "German",
            "it" => "Italian",
            "pt" => "Portuguese",
            _ => "English"
        };
    }

    // Response modelleri
    private class AzureOpenAIResponse
    {
        public Choice[]? Choices { get; set; }
        public Usage? Usage { get; set; }
    }

    private class Choice
    {
        public Message? Message { get; set; }
    }

    private class Message
    {
        public string? Content { get; set; }
    }

    private class Usage
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }
}
