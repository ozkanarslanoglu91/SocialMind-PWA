namespace SocialMind.Shared.Models;

/// <summary>
/// Desteklenen AI modelleri
/// </summary>
public enum AIProvider
{
    OpenAI,
    GoogleGemini,
    NVIDIA,
    GitHubModels,
    MicrosoftFoundry,
    AzureOpenAI
}

/// <summary>
/// AI modeli yapılandırması
/// </summary>
public class AIModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AIProvider Provider { get; set; }
    public string? Icon { get; set; }
    public decimal CostPerMillion { get; set; }
    public int ContextWindow { get; set; }
    public bool IsFree { get; set; }
    public string[] SupportedLanguages { get; set; } = [];
    public string[] Capabilities { get; set; } = [];
}

/// <summary>
/// AI servis yapılandırması
/// </summary>
public class AIServiceConfiguration
{
    public string ApiKey { get; set; } = string.Empty;
    public AIProvider Provider { get; set; }
    public string ModelId { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = [];
}

/// <summary>
/// AI ile üretilen içerik
/// </summary>
public class AIGeneratedContent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public AIProvider Provider { get; set; }
    public string ModelId { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Tüm AI modelleri
/// </summary>
public static class AIModelRegistry
{
    public static readonly AIModel[] AllModels =
    [
        // OpenAI
        new AIModel
        {
            Id = "gpt-4o",
            Name = "GPT-4o",
            Description = "En gelişmiş OpenAI modeli - metni, görüntüleri ve kodu işler",
            Provider = AIProvider.OpenAI,
            CostPerMillion = 2.50m,
            ContextWindow = 128000,
            IsFree = false,
            SupportedLanguages = ["en", "tr", "fr", "es", "de", "ja", "zh"],
            Capabilities = ["İçerik üretimi", "Görüntü analizi", "Kod üretimi", "Çeviri"]
        },
        new AIModel
        {
            Id = "gpt-4o-mini",
            Name = "GPT-4o Mini",
            Description = "Hızlı ve uygun maliyetli OpenAI modeli",
            Provider = AIProvider.OpenAI,
            CostPerMillion = 0.15m,
            ContextWindow = 128000,
            IsFree = false,
            SupportedLanguages = ["en", "tr", "fr", "es", "de"],
            Capabilities = ["İçerik üretimi", "Çeviri", "Duygu analizi"]
        },

        // Google Gemini
        new AIModel
        {
            Id = "gemini-1.5-pro",
            Name = "Gemini 1.5 Pro",
            Description = "Google'ın en güçlü modeli - 2M context window",
            Provider = AIProvider.GoogleGemini,
            CostPerMillion = 1.25m,
            ContextWindow = 2000000,
            IsFree = false,
            SupportedLanguages = ["en", "tr", "fr", "es", "de", "ja", "zh"],
            Capabilities = ["İçerik üretimi", "Görüntü analizi", "Kod üretimi"]
        },
        new AIModel
        {
            Id = "gemini-1.5-flash",
            Name = "Gemini 1.5 Flash",
            Description = "Hızlı yanıtlar ve çok dilli destek",
            Provider = AIProvider.GoogleGemini,
            CostPerMillion = 0.075m,
            ContextWindow = 1000000,
            IsFree = false,
            SupportedLanguages = ["en", "tr", "fr", "es", "de", "ja", "zh"],
            Capabilities = ["Hızlı yanıtlar", "Çok dilli destek"]
        },

        // NVIDIA
        new AIModel
        {
            Id = "llama-3.1-nemotron-70b",
            Name = "Llama 3.1 Nemotron 70B",
            Description = "NVIDIA'nın optimize edilmiş Meta modeli",
            Provider = AIProvider.NVIDIA,
            CostPerMillion = 0m,
            ContextWindow = 128000,
            IsFree = true,
            SupportedLanguages = ["en"],
            Capabilities = ["İçerik üretimi", "Gelişmiş instruction following"]
        },
        new AIModel
        {
            Id = "mistral-nemo-12b",
            Name = "Mistral NeMo 12B",
            Description = "Hızlı ve çok dilli NVIDIA optimize modeli",
            Provider = AIProvider.NVIDIA,
            CostPerMillion = 0m,
            ContextWindow = 128000,
            IsFree = true,
            SupportedLanguages = ["en", "tr", "fr", "es", "de", "ja", "zh"],
            Capabilities = ["Hızlı yanıtlar", "Çok dilli"]
        },

        // GitHub Models (Ücretsiz!)
        new AIModel
        {
            Id = "gpt-4o-github",
            Name = "GPT-4o (GitHub)",
            Description = "Premium OpenAI modeli - ÜCRETSİZ!",
            Provider = AIProvider.GitHubModels,
            CostPerMillion = 0m,
            ContextWindow = 128000,
            IsFree = true,
            SupportedLanguages = ["en", "tr", "fr", "es", "de", "ja", "zh"],
            Capabilities = ["İçerik üretimi", "Görüntü analizi", "Kod üretimi"]
        },
        new AIModel
        {
            Id = "gpt-4o-mini-github",
            Name = "GPT-4o Mini (GitHub)",
            Description = "Hızlı OpenAI modeli - ÜCRETSİZ!",
            Provider = AIProvider.GitHubModels,
            CostPerMillion = 0m,
            ContextWindow = 128000,
            IsFree = true,
            SupportedLanguages = ["en", "tr", "fr", "es", "de"],
            Capabilities = ["Hızlı içerik üretimi"]
        },
        new AIModel
        {
            Id = "phi-4",
            Name = "Phi-4",
            Description = "Microsoft'un kompakt modeli - ÜCRETSİZ!",
            Provider = AIProvider.GitHubModels,
            CostPerMillion = 0m,
            ContextWindow = 16000,
            IsFree = true,
            SupportedLanguages = ["en"],
            Capabilities = ["Kod üretimi", "Problem çözümü"]
        },
        new AIModel
        {
            Id = "llama-3.3-70b",
            Name = "Llama 3.3 70B",
            Description = "Meta'nın en son modeli - ÜCRETSİZ!",
            Provider = AIProvider.GitHubModels,
            CostPerMillion = 0m,
            ContextWindow = 128000,
            IsFree = true,
            SupportedLanguages = ["en", "tr"],
            Capabilities = ["İçerik üretimi", "Çok dilli"]
        },
        new AIModel
        {
            Id = "mistral-large",
            Name = "Mistral Large",
            Description = "Mistral AI'ın flagship modeli - ÜCRETSİZ!",
            Provider = AIProvider.GitHubModels,
            CostPerMillion = 0m,
            ContextWindow = 128000,
            IsFree = true,
            SupportedLanguages = ["en", "tr", "fr", "es", "de", "ja", "zh"],
            Capabilities = ["Çok dilli", "Gelişmiş akıl yürütme"]
        },
        new AIModel
        {
            Id = "cohere-command-r",
            Name = "Cohere Command R",
            Description = "İşe odaklı içerik için - ÜCRETSİZ!",
            Provider = AIProvider.GitHubModels,
            CostPerMillion = 0m,
            ContextWindow = 128000,
            IsFree = true,
            SupportedLanguages = ["en"],
            Capabilities = ["İş içeriği", "Hızlı yanıtlar"]
        },
        new AIModel
        {
            Id = "jamba-1.5",
            Name = "AI21 Jamba 1.5",
            Description = "Çok geniş context penceresi - 256K! - ÜCRETSİZ!",
            Provider = AIProvider.GitHubModels,
            CostPerMillion = 0m,
            ContextWindow = 256000,
            IsFree = true,
            SupportedLanguages = ["en"],
            Capabilities = ["Uzun dökümanlar", "Kapsamlı analiz"]
        },

        // Microsoft Foundry / Azure AI
        new AIModel
        {
            Id = "phi-4",
            Name = "Phi-4",
            Description = "Microsoft'un en yeni kompakt modeli - yüksek performans, düşük maliyet",
            Provider = AIProvider.MicrosoftFoundry,
            CostPerMillion = 0m,
            ContextWindow = 16000,
            IsFree = true,
            SupportedLanguages = ["en", "tr"],
            Capabilities = ["Kod üretimi", "Matematik", "Akıl yürütme", "Problem çözümü"]
        },
        new AIModel
        {
            Id = "phi-3.5-mini",
            Name = "Phi-3.5 Mini",
            Description = "Küçük ama güçlü - edge cihazlar için optimize edilmiş",
            Provider = AIProvider.MicrosoftFoundry,
            CostPerMillion = 0m,
            ContextWindow = 128000,
            IsFree = true,
            SupportedLanguages = ["en", "tr"],
            Capabilities = ["Hızlı yanıtlar", "Edge computing", "Düşük latency"]
        },
        new AIModel
        {
            Id = "phi-3-medium",
            Name = "Phi-3 Medium",
            Description = "Denge - performans ve verimlilik",
            Provider = AIProvider.MicrosoftFoundry,
            CostPerMillion = 0m,
            ContextWindow = 128000,
            IsFree = true,
            SupportedLanguages = ["en", "tr", "es", "fr", "de"],
            Capabilities = ["Çok dilli", "İçerik üretimi", "Analiz"]
        },
        new AIModel
        {
            Id = "llama-3.3-70b-instruct",
            Name = "Llama 3.3 70B Instruct",
            Description = "Meta'nın instruction-tuned modeli - Azure üzerinde",
            Provider = AIProvider.MicrosoftFoundry,
            CostPerMillion = 0.77m,
            ContextWindow = 128000,
            IsFree = false,
            SupportedLanguages = ["en", "tr", "es", "fr", "de", "it", "pt"],
            Capabilities = ["Gelişmiş akıl yürütme", "Kod üretimi", "Çok dilli içerik"]
        },
        new AIModel
        {
            Id = "mistral-large-2411",
            Name = "Mistral Large 2411",
            Description = "Mistral AI'ın en gelişmiş modeli - Azure üzerinde",
            Provider = AIProvider.MicrosoftFoundry,
            CostPerMillion = 2m,
            ContextWindow = 128000,
            IsFree = false,
            SupportedLanguages = ["en", "fr", "es", "de", "it", "tr"],
            Capabilities = ["Function calling", "JSON mode", "Gelişmiş akıl yürütme"]
        },
        new AIModel
        {
            Id = "mistral-small",
            Name = "Mistral Small",
            Description = "Hızlı ve uygun maliyetli Mistral modeli",
            Provider = AIProvider.MicrosoftFoundry,
            CostPerMillion = 0.2m,
            ContextWindow = 32000,
            IsFree = false,
            SupportedLanguages = ["en", "fr", "es", "de", "it"],
            Capabilities = ["Hızlı yanıtlar", "Düşük maliyet", "Çok dilli"]
        },

        // Azure OpenAI (Enterprise)
        new AIModel
        {
            Id = "gpt-4o-azure",
            Name = "GPT-4o (Azure)",
            Description = "Azure OpenAI Service - Enterprise özelliklerle",
            Provider = AIProvider.AzureOpenAI,
            CostPerMillion = 2.50m,
            ContextWindow = 128000,
            IsFree = false,
            SupportedLanguages = ["en", "tr", "fr", "es", "de", "ja", "zh"],
            Capabilities = ["İçerik üretimi", "Görüntü analizi", "Enterprise compliance", "Private network"]
        },
        new AIModel
        {
            Id = "gpt-4-turbo-azure",
            Name = "GPT-4 Turbo (Azure)",
            Description = "Azure OpenAI - Yüksek throughput ve enterprise güvenlik",
            Provider = AIProvider.AzureOpenAI,
            CostPerMillion = 10m,
            ContextWindow = 128000,
            IsFree = false,
            SupportedLanguages = ["en", "tr", "fr", "es", "de", "ja", "zh"],
            Capabilities = ["Gelişmiş akıl yürütme", "Enterprise SLA", "Data residency", "Compliance"]
        },
        new AIModel
        {
            Id = "gpt-35-turbo-azure",
            Name = "GPT-3.5 Turbo (Azure)",
            Description = "Azure OpenAI - Hızlı ve uygun maliyetli",
            Provider = AIProvider.AzureOpenAI,
            CostPerMillion = 0.5m,
            ContextWindow = 16000,
            IsFree = false,
            SupportedLanguages = ["en", "tr", "fr", "es", "de"],
            Capabilities = ["Hızlı yanıtlar", "Düşük maliyet", "Enterprise güvenlik"]
        }
    ];

    public static AIModel? GetModel(string modelId)
    {
        return AllModels.FirstOrDefault(m => m.Id == modelId);
    }

    public static IEnumerable<AIModel> GetByProvider(AIProvider provider)
    {
        return AllModels.Where(m => m.Provider == provider);
    }

    public static IEnumerable<AIModel> GetFreeModels()
    {
        return AllModels.Where(m => m.IsFree);
    }
}
