using GeminiMauiApp.Models;

namespace GeminiMauiApp.Services;

public class SettingsService
{
    private const string ApiKeyPref    = "gemini_api_key";
    private const string ModelPref     = "gemini_model";
    private const string TtsEnabledPref = "tts_enabled";

    public string ApiKey
    {
        get => Preferences.Default.Get(ApiKeyPref, string.Empty);
        set => Preferences.Default.Set(ApiKeyPref, value);
    }

    public string SelectedModel
    {
        get => Preferences.Default.Get(ModelPref, GeminiModels.Default);
        set => Preferences.Default.Set(ModelPref, value);
    }

    public bool TtsEnabled
    {
        get => Preferences.Default.Get(TtsEnabledPref, true);
        set => Preferences.Default.Set(TtsEnabledPref, value);
    }

    public bool HasApiKey => !string.IsNullOrWhiteSpace(ApiKey);
}
