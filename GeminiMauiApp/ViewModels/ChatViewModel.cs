using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GeminiMauiApp.Models;
using GeminiMauiApp.Services;

namespace GeminiMauiApp.ViewModels;

public partial class ChatViewModel : BaseViewModel
{
    private readonly GeminiService _gemini;
    private readonly SpeechService _speech;
    private readonly SettingsService _settings;

    private CancellationTokenSource? _sttCts;


    [ObservableProperty]
    private string _userInput = string.Empty;

    [ObservableProperty]
    private bool _isListening;

    [ObservableProperty]
    private bool _isThinking;

    [ObservableProperty]
    private string _listeningLabel = "Hablar";

    [ObservableProperty]
    private ImageSource? _previewImage;

    [ObservableProperty]
    private bool _hasPreviewImage;

    private byte[]? _pendingImageBytes;

    public ObservableCollection<ChatMessage> Messages { get; } = [];


    public ChatViewModel(GeminiService gemini, SpeechService speech, SettingsService settings)
    {
        _gemini   = gemini;
        _speech   = speech;
        _settings = settings;
        Title     = "Gemini Chat";
    }


    [RelayCommand]
    private async Task SendAsync()
    {
        var text = UserInput.Trim();
        if (string.IsNullOrEmpty(text) && !HasPreviewImage) return;
        if (!_settings.HasApiKey)
        {
            await Shell.Current.DisplayAlert(
                "Sin API Key",
                "Por favor configura tu API Key de Gemini en Ajustes.",
                "Ir a Ajustes");
            return;
        }

        var userMsg = new ChatMessage
        {
            Role      = MessageRole.User,
            Content   = text,
            HasImage  = HasPreviewImage,
            ImageData = _pendingImageBytes
        };

        Messages.Add(userMsg);
        UserInput = string.Empty;
        ClearImage();

        var thinkingMsg = new ChatMessage
        {
            Role    = MessageRole.Assistant,
            Content = "..."
        };
        Messages.Add(thinkingMsg);
        IsThinking = true;

        try
        {
            var history = Messages
                .Where(m => m != thinkingMsg)
                .ToList();

            var reply = await _gemini.SendMessageAsync(history);

            Messages.Remove(thinkingMsg);
            var assistantMsg = new ChatMessage
            {
                Role    = MessageRole.Assistant,
                Content = reply
            };
            Messages.Add(assistantMsg);

            if (_settings.TtsEnabled)
                _ = _speech.SpeakAsync(reply);
        }
        catch (Exception ex)
        {
            Messages.Remove(thinkingMsg);
            Messages.Add(new ChatMessage
            {
                Role    = MessageRole.Assistant,
                Content = $"⚠️ {ex.Message}"
            });
        }
        finally
        {
            IsThinking = false;
        }
    }

    [RelayCommand]
    private async Task ToggleListenAsync()
    {
        if (IsListening)
        {
            await StopListeningAsync();
            return;
        }

        IsListening    = true;
        ListeningLabel = "Detener";

        _sttCts = new CancellationTokenSource();
        var progress = new Progress<string>(partial =>
        {
            UserInput = partial;
        });

        try
        {
            var result = await _speech.ListenAsync(progress, _sttCts.Token);
            if (!string.IsNullOrWhiteSpace(result))
                UserInput = result;
        }
        catch (PermissionException)
        {
            await Shell.Current.DisplayAlert(
                "Permiso",
                "Se necesita permiso de micrófono para usar el dictado.",
                "OK");
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error STT", ex.Message, "OK");
        }
        finally
        {
            IsListening    = false;
            ListeningLabel = "Hablar";
        }
    }

    [RelayCommand]
    private async Task AttachImageAsync()
    {
        try
        {
            var result = await MediaPicker.Default.CapturePhotoAsync();
            if (result == null) return;

            await using var stream = await result.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            _pendingImageBytes = ms.ToArray();

            PreviewImage  = ImageSource.FromStream(() => new MemoryStream(_pendingImageBytes));
            HasPreviewImage = true;
        }
        catch (FeatureNotSupportedException)
        {
            await Shell.Current.DisplayAlert("Cámara", "La cámara no está disponible.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error cámara", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private void ClearImage()
    {
        _pendingImageBytes = null;
        PreviewImage       = null;
        HasPreviewImage    = false;
    }

    [RelayCommand]
    private void ClearChat()
    {
        Messages.Clear();
        _ = _speech.StopSpeakingAsync();
    }


    private async Task StopListeningAsync()
    {
        if (_sttCts is { IsCancellationRequested: false })
            await _sttCts.CancelAsync();
    }
}
