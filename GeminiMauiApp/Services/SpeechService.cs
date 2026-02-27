using CommunityToolkit.Maui.Media;

namespace GeminiMauiApp.Services;

public class SpeechService
{
    private readonly ISpeechToText _stt;
    private CancellationTokenSource? _ttsCts;

    public SpeechService(ISpeechToText stt)
    {
        _stt = stt;
    }


    public async Task SpeakAsync(string text, CancellationToken ct = default)
    {
        await StopSpeakingAsync();
        _ttsCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        var settings = new SpeechOptions { Volume = 1f, Pitch = 1f };

        var locales = await TextToSpeech.Default.GetLocalesAsync();
        var spanish = locales.FirstOrDefault(l =>
            l.Language.StartsWith("es", StringComparison.OrdinalIgnoreCase));
        if (spanish != null)
            settings.Locale = spanish;

        await TextToSpeech.Default.SpeakAsync(text, settings, _ttsCts.Token);
    }

    public async Task StopSpeakingAsync()
    {
        if (_ttsCts is { IsCancellationRequested: false })
            await _ttsCts.CancelAsync();
        _ttsCts?.Dispose();
        _ttsCts = null;
    }


    public async Task<string?> ListenAsync(
        IProgress<string>? partialResults,
        CancellationToken ct)
    {
        bool hasPermission = await CheckMicrophonePermissionAsync();
        if (!hasPermission)
            throw new PermissionException("Permiso de micrófono denegado.");

        var result = await _stt.ListenAsync(
            System.Globalization.CultureInfo.CurrentCulture,
            partialResults,
            ct);

        if (result.IsSuccessful)
            return result.Text;

        throw new InvalidOperationException(
            result.Exception?.Message ?? "Error desconocido de STT.");
    }

    private static async Task<bool> CheckMicrophonePermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();
        if (status == PermissionStatus.Granted) return true;
        status = await Permissions.RequestAsync<Permissions.Microphone>();
        return status == PermissionStatus.Granted;
    }
}