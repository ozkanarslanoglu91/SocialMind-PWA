using SocialMind.Shared.Models;
using SocialMind.Shared.Services;
using System.Text.Json;

namespace SocialMind.Web.Services;

/// <summary>
/// Mock Settings servisi - test ve geliştirme için
/// LocalStorage simülasyonu kullanır
/// </summary>
public class MockSettingsService : ISettingsService
{
    private readonly Dictionary<string, string> _apiKeys = new();
    private readonly Dictionary<string, object> _preferences = new();

    public MockSettingsService()
    {
        InitializeDefaults();
    }

    private void InitializeDefaults()
    {
        // Varsayılan tercihler
        _preferences["theme"] = "light";
        _preferences["language"] = "tr";
        _preferences["notifications"] = true;
        _preferences["autoPublish"] = false;
        _preferences["defaultPlatforms"] = new[] { "Twitter", "LinkedIn" };
    }

    public Task<bool> SaveApiKeyAsync(AIProvider provider, string apiKey)
    {
        var key = $"apikey_{provider}";
        _apiKeys[key] = apiKey;
        return Task.FromResult(true);
    }

    public Task<string?> GetApiKeyAsync(AIProvider provider)
    {
        var key = $"apikey_{provider}";
        if (_apiKeys.TryGetValue(key, out var apiKey))
        {
            return Task.FromResult<string?>(apiKey);
        }
        return Task.FromResult<string?>(null);
    }

    public async Task<bool> ValidateApiKeyAsync(AIProvider provider, string apiKey)
    {
        // Mock validasyon - basit kontrol
        if (string.IsNullOrEmpty(apiKey) || apiKey.Length < 10)
        {
            return false;
        }

        // API key formatını kontrol et
        return provider switch
        {
            AIProvider.OpenAI => apiKey.StartsWith("sk-"),
            AIProvider.GoogleGemini => apiKey.StartsWith("AI"),
            AIProvider.NVIDIA => apiKey.StartsWith("nvapi-"),
            AIProvider.GitHubModels => apiKey.StartsWith("ghp_"),
            _ => true
        };
    }

    public Task<bool> DeleteApiKeyAsync(AIProvider provider)
    {
        var key = $"apikey_{provider}";
        if (_apiKeys.ContainsKey(key))
        {
            _apiKeys.Remove(key);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<Dictionary<string, object>> GetUserPreferencesAsync()
    {
        return Task.FromResult(new Dictionary<string, object>(_preferences));
    }

    public Task<bool> SaveUserPreferencesAsync(Dictionary<string, object> preferences)
    {
        foreach (var kvp in preferences)
        {
            _preferences[kvp.Key] = kvp.Value;
        }
        return Task.FromResult(true);
    }

    public Task<T?> GetPreferenceAsync<T>(string key)
    {
        if (_preferences.TryGetValue(key, out var value))
        {
            try
            {
                if (value is T typedValue)
                {
                    return Task.FromResult<T?>(typedValue);
                }

                // JSON serileştirme ile type conversion
                var json = JsonSerializer.Serialize(value);
                var result = JsonSerializer.Deserialize<T>(json);
                return Task.FromResult(result);
            }
            catch
            {
                return Task.FromResult<T?>(default);
            }
        }
        return Task.FromResult<T?>(default);
    }

    public Task<bool> SavePreferenceAsync<T>(string key, T value)
    {
        if (value != null)
        {
            _preferences[key] = value;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}
