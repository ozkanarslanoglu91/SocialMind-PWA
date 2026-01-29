# Katkıda Bulunma Rehberi

SocialMind projesine katkıda bulunmak istediğiniz için teşekkür ederiz! 🎉

## 🚀 Nasıl Katkıda Bulunabilirim?

### 1️⃣ Hata Bildirimi (Bug Report)

Bir hata bulduysanız:

- [Issues](https://github.com/ultrarslanoglu/SocialMind/issues) sayfasından yeni bir issue açın
- Hatayı detaylı açıklayın
- Tekrar üretme adımlarını ekleyin
- Ekran görüntüleri ekleyin (varsa)
- Ortam bilgilerinizi belirtin (.NET versiyon, OS, vb.)

### 2️⃣ Özellik Önerisi (Feature Request)

Yeni bir özellik önerisi için:

- [Discussions](https://github.com/ultrarslanoglu/SocialMind/discussions) bölümünde tartışma başlatın
- Özelliğin ne işe yarayacağını açıklayın
- Örnek kullanım senaryoları ekleyin
- Varsa mockup/tasarım ekleyin

### 3️⃣ Kod Katkısı (Pull Request)

#### Başlamadan Önce

1. Projeyi fork edin
2. Development branch'inden yeni bir branch oluşturun
3. Değişikliklerinizi bu branch'te yapın

#### Branch İsimlendirme

```
feature/feature-name     # Yeni özellik
bugfix/bug-description   # Hata düzeltme
hotfix/critical-fix      # Acil düzeltme
docs/documentation-update # Dokümantasyon
refactor/code-improvement # Kod iyileştirme
```

#### Commit Mesajları

```
feat: Yeni özellik açıklaması
fix: Hata düzeltme açıklaması
docs: Dokümantasyon güncellemesi
style: Kod formatlama
refactor: Kod yeniden yapılandırma
test: Test ekleme/güncelleme
chore: Bakım işleri
```

Örnek:

```bash
git commit -m "feat: Add Instagram OAuth integration"
git commit -m "fix: Character counter not updating correctly"
git commit -m "docs: Update API configuration guide"
```

#### Pull Request Süreci

1. Kodunuzun çalıştığından emin olun (`dotnet build`)
2. Testleri çalıştırın (`dotnet test`)
3. README veya dokümantasyonu güncelleyin (gerekirse)
4. PR açın ve detaylı açıklama ekleyin
5. Review sürecini bekleyin

## 📋 Kod Standartları

### C# Kod Stili

```csharp
// ✅ Doğru
public class PostService : IPostService
{
    private readonly ILogger<PostService> _logger;
    
    public PostService(ILogger<PostService> logger)
    {
        _logger = logger;
    }
    
    public async Task<Post> CreatePostAsync(Post post)
    {
        ArgumentNullException.ThrowIfNull(post);
        
        // Implementation
        return post;
    }
}

// ❌ Yanlış
public class postservice 
{
    public Post CreatePost(Post post) 
    {
        return post;
    }
}
```

### Naming Conventions

- **Classes**: PascalCase (`PostService`, `AIModel`)
- **Methods**: PascalCase (`CreatePostAsync`, `GetAllPosts`)
- **Properties**: PascalCase (`PostId`, `CreatedAt`)
- **Private fields**: _camelCase (`_logger`, `_dbContext`)
- **Parameters**: camelCase (`postId`, `userId`)
- **Constants**: PascalCase (`MaxRetryCount`)

### Async/Await Kullanımı

```csharp
// ✅ Doğru
public async Task<Post> GetPostAsync(string id)
{
    return await _dbContext.Posts.FindAsync(id);
}

// ❌ Yanlış
public Post GetPost(string id)
{
    return _dbContext.Posts.Find(id);
}
```

### Null Safety

```csharp
// ✅ Nullable reference types kullanın
public string? OptionalField { get; set; }
public string RequiredField { get; set; } = string.Empty;

// ✅ Null kontrolü
ArgumentNullException.ThrowIfNull(parameter);
if (value is null) return;
```

## 🧪 Test Yazma

### Unit Test Örneği

```csharp
public class PostServiceTests
{
    [Fact]
    public async Task CreatePostAsync_ValidPost_ReturnsCreatedPost()
    {
        // Arrange
        var service = new PostService();
        var post = new Post { Title = "Test" };
        
        // Act
        var result = await service.CreatePostAsync(post);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.Title);
    }
}
```

## 📚 Dokümantasyon

### XML Comments

```csharp
/// <summary>
/// Creates a new post in the system
/// </summary>
/// <param name="post">The post to create</param>
/// <returns>The created post with generated ID</returns>
/// <exception cref="ArgumentNullException">Thrown when post is null</exception>
public async Task<Post> CreatePostAsync(Post post)
{
    // Implementation
}
```

### README Güncellemeleri

- Yeni özellik eklediyseniz README'ye ekleyin
- API değişiklikleri için dokümantasyon güncelleyin
- Kurulum adımları değiştiyse güncelleyin

## 🏗️ Proje Yapısı Kuralları

### Dosya Organizasyonu

```
SocialMind.Shared/
├── Models/           # Sadece model sınıfları
├── Services/         # Interface ve implementasyonlar
├── Components/       # Razor components
└── Utils/           # Yardımcı sınıflar
```

### Yeni Model Ekleme

```csharp
// Models/YourModel.cs
namespace SocialMind.Shared.Models;

public class YourModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // ...
}
```

### Yeni Service Ekleme

```csharp
// Services/IYourService.cs
public interface IYourService
{
    Task<Result> DoSomethingAsync();
}

// Services/YourService.cs
public class YourService : IYourService
{
    public async Task<Result> DoSomethingAsync()
    {
        // Implementation
    }
}
```

## 🔍 Code Review Kriterleri

PR'ınız şu kriterlere göre değerlendirilecek:

### ✅ Zorunlu

- [ ] Kod derlenebiliyor
- [ ] Tüm testler geçiyor
- [ ] Yeni kod için testler yazılmış
- [ ] Dokümantasyon güncellenmiş
- [ ] Breaking change yok (veya belirtilmiş)
- [ ] Kod standartlarına uygun

### 📝 İsteğe Bağlı

- [ ] Performance iyileştirmesi yapılmış
- [ ] Error handling eklenmiş
- [ ] Logging eklenmiş
- [ ] Güvenlik açıkları kontrol edilmiş

## 🐛 Hata Ayıklama İpuçları

### Logging

```csharp
_logger.LogInformation("Post created: {PostId}", post.Id);
_logger.LogWarning("API rate limit approaching: {RemainingCalls}", remaining);
_logger.LogError(ex, "Failed to publish post: {PostId}", postId);
```

### Exception Handling

```csharp
try
{
    await _service.DoSomethingAsync();
}
catch (HttpRequestException ex)
{
    _logger.LogError(ex, "HTTP request failed");
    throw new ServiceException("Service unavailable", ex);
}
```

## 📞 İletişim

Sorularınız için:

- 💬 [GitHub Discussions](https://github.com/ultrarslanoglu/SocialMind/discussions)
- 🐛 [GitHub Issues](https://github.com/ultrarslanoglu/SocialMind/issues)

## 🎉 İlk Katkınız mı?

Hoş geldiniz! Şu etiketlere bakın:

- `good first issue` - Yeni başlayanlar için
- `help wanted` - Yardım arıyoruz
- `documentation` - Dokümantasyon katkıları

## 📜 Davranış Kuralları

- 🤝 Saygılı olun
- 💬 Yapıcı eleştiri yapın
- 🎯 Konuyla ilgili kalın
- 🌍 Çeşitliliğe saygı gösterin

## 🙏 Teşekkürler

Katkılarınız için teşekkür ederiz! Her katkı, SocialMind'ı daha iyi hale getirir. ❤️
