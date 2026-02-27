using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using GeminiMauiApp.Models;

namespace GeminiMauiApp.Services;

public class GeminiService
{
    private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models";

    private readonly HttpClient _http;
    private readonly SettingsService _settings;

    public GeminiService(HttpClient http, SettingsService settings)
    {
        _http = http;
        _settings = settings;
        _http.Timeout = TimeSpan.FromSeconds(60);
    }


    public async Task<string> SendMessageAsync(
        List<ChatMessage> history,
        CancellationToken ct = default)
    {
        if (!_settings.HasApiKey)
            throw new InvalidOperationException("API Key no configurada. Ve a Ajustes.");

        var contents = BuildContents(history);

        var request = new GeminiRequest
        {
            Contents = contents,
            GenerationConfig = new GenerationConfig
            {
                Temperature = 0.9,
                MaxOutputTokens = 2048
            }
        };

        var url = $"{BaseUrl}/{_settings.SelectedModel}:generateContent?key={_settings.ApiKey}";
        var json = JsonSerializer.Serialize(request);
        using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync(url, httpContent, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            try
            {
                var errResp = JsonSerializer.Deserialize<GeminiResponse>(body);
                var msg = errResp?.Error?.Message ?? $"HTTP {(int)response.StatusCode}";
                throw new HttpRequestException($"Error de Gemini: {msg}");
            }
            catch (JsonException)
            {
                throw new HttpRequestException($"Error HTTP {(int)response.StatusCode}: {body}");
            }
        }

        var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(body);
        var text = geminiResponse?.GetText();

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("Gemini devolvió una respuesta vacía.");

        return text;
    }

    private static List<GeminiContent> BuildContents(List<ChatMessage> history)
    {
        var contents = new List<GeminiContent>();

        foreach (var msg in history)
        {
            var parts = new List<GeminiPart>();

            if (msg.IsUser && msg.HasImage && msg.ImageData != null)
            {
                parts.Add(new GeminiPart
                {
                    InlineData = new InlineData
                    {
                        MimeType = "image/jpeg",
                        Data = Convert.ToBase64String(msg.ImageData)
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(msg.Content))
            {
                parts.Add(new GeminiPart { Text = msg.Content });
            }

            if (parts.Count == 0) continue;

            contents.Add(new GeminiContent
            {
                Role = msg.IsUser ? "user" : "model",
                Parts = parts
            });
        }

        return contents;
    }
}
