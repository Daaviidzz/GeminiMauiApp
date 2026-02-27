using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GeminiMauiApp.Models;
using GeminiMauiApp.Services;

namespace GeminiMauiApp.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly SettingsService _settings;

    [ObservableProperty]
    private string _apiKey = string.Empty;

    [ObservableProperty]
    private string _selectedModel = string.Empty;

    [ObservableProperty]
    private bool _ttsEnabled;

    [ObservableProperty]
    private bool _isApiKeyVisible;

    public ObservableCollection<string> AvailableModels { get; } =
        new(GeminiModels.Available);

    public SettingsViewModel(SettingsService settings)
    {
        _settings     = settings;
        Title         = "Ajustes";

        // Load persisted values
        ApiKey        = _settings.ApiKey;
        SelectedModel = _settings.SelectedModel;
        TtsEnabled    = _settings.TtsEnabled;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            await Shell.Current.DisplayAlert("Atención", "La API Key no puede estar vacía.", "OK");
            return;
        }

        _settings.ApiKey        = ApiKey.Trim();
        _settings.SelectedModel = SelectedModel;
        _settings.TtsEnabled    = TtsEnabled;

        await Shell.Current.DisplayAlert("✓ Guardado", "Ajustes guardados correctamente.", "OK");
    }

    [RelayCommand]
    private void ToggleApiKeyVisibility()
    {
        IsApiKeyVisible = !IsApiKeyVisible;
    }

    partial void OnSelectedModelChanged(string value)
    {
        _settings.SelectedModel = value;
    }

    partial void OnTtsEnabledChanged(bool value)
    {
        _settings.TtsEnabled = value;
    }
}
