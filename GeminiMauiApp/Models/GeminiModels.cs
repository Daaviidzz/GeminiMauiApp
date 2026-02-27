using System.Text.Json.Serialization;

namespace GeminiMauiApp.Models;


public class GeminiRequest
{
    [JsonPropertyName("contents")]
    public List<GeminiContent> Contents { get; set; } = [];

    [JsonPropertyName("generationConfig")]
    public GenerationConfig? GenerationConfig { get; set; }

    [JsonPropertyName("systemInstruction")]
    public GeminiContent? SystemInstruction { get; set; }
}

public class GeminiContent
{
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("parts")]
    public List<GeminiPart> Parts { get; set; } = [];
}

public class GeminiPart
{
    [JsonPropertyName("text")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Text { get; set; }

    [JsonPropertyName("inlineData")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public InlineData? InlineData { get; set; }
}

public class InlineData
{
    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; } = "image/jpeg";

    [JsonPropertyName("data")]
    public string Data { get; set; } = string.Empty;
}

public class GenerationConfig
{
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 0.9;

    [JsonPropertyName("maxOutputTokens")]
    public int MaxOutputTokens { get; set; } = 2048;
}


public class GeminiResponse
{
    [JsonPropertyName("candidates")]
    public List<GeminiCandidate>? Candidates { get; set; }

    [JsonPropertyName("error")]
    public GeminiError? Error { get; set; }

    public string? GetText() =>
        Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
}

public class GeminiCandidate
{
    [JsonPropertyName("content")]
    public GeminiContent? Content { get; set; }

    [JsonPropertyName("finishReason")]
    public string? FinishReason { get; set; }
}

public class GeminiError
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public int Code { get; set; }
}


public static class GeminiModels
{
    public static readonly List<string> Available = new()
    {
        "gemini-2.5-flash",
        "gemini-2.0-flash-lite",
        "gemini-1.5-flash",
        "gemini-1.5-flash-8b",
        "gemini-1.5-pro",
    };

    public const string Default = "gemini-2.0-flash";
}
