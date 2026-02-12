# Microsoft Foundry & Azure OpenAI Entegrasyonu

## 🎯 Genel Bakış

SocialMind projesi, Microsoft'un AI ekosistemiyle entegre edilmiştir:

- **Microsoft Foundry**: Phi serisi modeller ve hosted open-source modeller (Llama, Mistral)
- **Azure OpenAI**: Enterprise özellikleriyle OpenAI modelleri (compliance, SLA, private endpoints)

## 📦 Dahil Edilen Modeller

### Microsoft Foundry Modelleri

1. **Phi-4** (16K context, ÜCRETSİZ)
   - Microsoft'un en son küçük dil modeli
   - Hızlı ve edge cihazlar için optimize

2. **Phi-3.5 Mini** (128K context, ÜCRETSİZ)
   - Geniş context window
   - Mobil ve edge deployment için ideal

3. **Phi-3 Medium** (128K context, ÜCRETSİZ)
   - Dengeli performans
   - Orta ölçekli uygulamalar için

4. **Llama 3.3 70B Instruct** ($0.77/1M token)
   - Meta'nın güçlü modeli Azure'da host edilmiş
   - Karmaşık görevler için

5. **Mistral Large 2411** ($2.00/1M token)
   - En gelişmiş Mistral modeli
   - Profesyonel içerik üretimi

6. **Mistral Small** ($0.20/1M token)
   - Hızlı ve uygun maliyetli
   - Basit içerik görevleri için

### Azure OpenAI Modelleri

1. **GPT-4o Azure** ($2.50/1M token)
   - Enterprise compliance
   - Gelişmiş reasoning ve multimodal

2. **GPT-4 Turbo Azure** ($10.00/1M token)
   - Yüksek throughput
   - 128K context window

3. **GPT-3.5 Turbo Azure** ($0.50/1M token)
   - Hızlı ve ekonomik
   - Basit görevler için

## 🔧 Kurulum

### 1. API Anahtarları

#### Microsoft Foundry

1. [GitHub Models](https://github.com/marketplace/models) üzerinden ücretsiz erişim
2. API anahtarınızı kopyalayın
3. `appsettings.json` içinde güncelleyin:

```json
"AIProviders": {
  "MicrosoftFoundry": {
    "ApiKey": "YOUR_GITHUB_TOKEN",
    "BaseUrl": "https://models.inference.ai.azure.com",
    "DefaultModel": "phi-4"
  }
}
```

#### Azure OpenAI

1. [Azure Portal](https://portal.azure.com) üzerinden Azure OpenAI kaynağı oluşturun
2. Model deployment yapın (örn: gpt-4o)
3. Endpoint ve API key'i alın
4. `appsettings.json` içinde güncelleyin:

```json
"AIProviders": {
  "AzureOpenAI": {
    "ApiKey": "YOUR_AZURE_OPENAI_KEY",
    "Endpoint": "https://YOUR-RESOURCE.openai.azure.com",
    "DeploymentName": "gpt-4o",
    "ApiVersion": "2024-02-15-preview"
  }
}
```

### 2. Service Seçimi

`Program.cs` içinde hangi AI servisini kullanacağınızı seçin:

```csharp
// Mock servis kullanımı (Development)
var enableMockServices = builder.Configuration.GetValue<bool>("AppSettings:EnableMockServices", true);

if (enableMockServices)
{
    builder.Services.AddScoped<IAIService, MockAIService>();
}
else
{
    // Production için gerçek servis
    builder.Services.AddScoped<IAIService, MicrosoftFoundryAIService>();
    // Ya da
    builder.Services.AddScoped<IAIService, AzureOpenAIService>();
}
```

### 3. Named Service Pattern (İsteğe Bağlı)

Birden fazla AI provider'ı dinamik olarak kullanmak için:

```csharp
// Program.cs içinde
builder.Services.AddKeyedScoped<IAIService, MicrosoftFoundryAIService>("MicrosoftFoundry");
builder.Services.AddKeyedScoped<IAIService, AzureOpenAIService>("AzureOpenAI");

// Component içinde kullanım
@inject IServiceProvider ServiceProvider

private async Task GenerateWithProvider(string provider)
{
    var aiService = ServiceProvider.GetKeyedService<IAIService>(provider);
    var content = await aiService.GenerateContentAsync(prompt, modelId);
}
```

## 🚀 Kullanım

### Basit İçerik Oluşturma

```csharp
@inject IAIService AIService

private async Task GenerateContent()
{
    var content = await AIService.GenerateContentAsync(
        prompt: "Yapay zeka hakkında ilgi çekici bir tweet yaz",
        modelId: "phi-4",
        language: "tr"
    );

    Console.WriteLine(content.Content);
    Console.WriteLine($"Kullanılan token: {content.Metadata["tokensUsed"]}");
}
```

### Çoklu Varyant Oluşturma

```csharp
var contents = await AIService.GenerateMultipleContentAsync(
    prompt: "LinkedIn için bir post yaz",
    modelId: "mistral-large-2411",
    count: 3
);

foreach (var content in contents)
{
    Console.WriteLine($"Varyant {contents.IndexOf(content) + 1}: {content.Content}");
}
```

### Platform Optimizasyonu

```csharp
var optimized = await AIService.OptimizeContentAsync(
    content: "Uzun bir içerik metni...",
    platform: SocialPlatform.Twitter,
    modelId: "phi-4"
);

Console.WriteLine($"Twitter için optimize edildi: {optimized}");
```

### Hashtag Oluşturma

```csharp
var hashtags = await AIService.GenerateHashtagsAsync(
    content: "Yapay zeka sosyal medya yönetimini kolaylaştırıyor",
    platform: SocialPlatform.Instagram,
    count: 10
);

Console.WriteLine($"Önerilen hashtagler: #{string.Join(" #", hashtags)}");
```

## 💰 Maliyet Karşılaştırması

| Model                   | Fiyat (1M token) | Kullanım Senaryosu                    |
| ----------------------- | ---------------- | ------------------------------------- |
| **Phi-4**               | ÜCRETSİZ         | Test, development, prototipleme       |
| **Phi-3.5 Mini**        | ÜCRETSİZ         | Mobil uygulamalar, edge computing     |
| **Mistral Small**       | $0.20            | Basit içerik üretimi, günlük görevler |
| **GPT-3.5 Turbo Azure** | $0.50            | Hızlı yanıtlar, basit içerikler       |
| **Llama 3.3 70B**       | $0.77            | Karmaşık içerikler, özel görevler     |
| **Mistral Large**       | $2.00            | Profesyonel içerik, yüksek kalite     |
| **GPT-4o Azure**        | $2.50            | Enterprise uygulamalar, compliance    |
| **GPT-4 Turbo Azure**   | $10.00           | Yüksek throughput, kritik işlemler    |

## 🏢 Enterprise Özellikler (Azure OpenAI)

1. **Compliance ve Sertifikalar**
   - SOC 2, ISO 27001, HIPAA, GDPR uyumlu
   - Avrupa veri yerleşimi seçeneği

2. **Güvenlik**
   - Private endpoints (VNet entegrasyonu)
   - Azure Active Directory authentication
   - Managed identities

3. **SLA ve Destek**
   - %99.9 uptime garantisi
   - 7/24 Microsoft desteği
   - Premium support seçenekleri

4. **Yönetim**
   - Azure Monitor entegrasyonu
   - Cost management ve budget alerts
   - Usage analytics ve reporting

## 🔍 Model Seçim Rehberi

### Geliştirme ve Test

- **Öneri**: Phi-4 veya Phi-3.5 Mini (ücretsiz)
- **Neden**: Sınırsız kullanım, hızlı iterasyon

### Startup/Küçük Proje

- **Öneri**: Mistral Small veya GPT-3.5 Turbo Azure
- **Neden**: Düşük maliyet, yeterli performans

### Orta Ölçekli Proje

- **Öneri**: Llama 3.3 70B veya Mistral Large
- **Neden**: Kalite-maliyet dengesi

### Enterprise/Kurumsal

- **Öneri**: GPT-4o Azure veya GPT-4 Turbo Azure
- **Neden**: Compliance, SLA, özel destek

## 🛠️ Troubleshooting

### API Key Hatası

```
Error: 401 Unauthorized
```

**Çözüm**: `appsettings.json` içinde API key'inizi kontrol edin

### Rate Limit Hatası

```
Error: 429 Too Many Requests
```

**Çözüm**: İstekler arasına `await Task.Delay(1000)` ekleyin

### Timeout Hatası

```
Error: Request timeout
```

**Çözüm**: `HttpClient.Timeout` değerini artırın (Program.cs)

### Model Bulunamadı Hatası

```
Error: Model not found
```

**Çözüm**: Azure OpenAI'da deployment oluşturulduğunu doğrulayın

## 📚 Kaynaklar

- [Microsoft Foundry Documentation](https://github.com/marketplace/models)
- [Azure OpenAI Service Documentation](https://learn.microsoft.com/azure/ai-services/openai/)
- [Phi Models Overview](https://azure.microsoft.com/blog/introducing-phi-4-microsoft-most-capable-ai-models/)
- [Azure AI Pricing](https://azure.microsoft.com/pricing/details/cognitive-services/openai-service/)

## 🔄 Sonraki Adımlar

1. ✅ Model registry'ye Foundry modelleri eklendi
2. ✅ Service implementasyonları oluşturuldu
3. ✅ Configuration yapılandırıldı
4. ⏳ API anahtarlarını appsettings.json'a ekleyin
5. ⏳ Mock moddan real mode'a geçiş yapın
6. ⏳ Usage tracking ve cost monitoring ekleyin
7. ⏳ Caching mekanizması ekleyin (maliyeti düşürmek için)

## 💡 İpuçları

1. **Development**: Mock servis kullanın, gerçek API çağrıları yapmayın
2. **Testing**: Phi-4 kullanın (ücretsiz)
3. **Production**: Kullanım senaryonuza göre model seçin
4. **Caching**: Aynı promptlar için cache kullanın
5. **Monitoring**: Token kullanımını takip edin
6. **Fallback**: Birincil provider başarısız olursa fallback provider kullanın
