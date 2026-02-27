namespace GeminiMauiApp.Models;

public enum MessageRole
{
    User,
    Assistant
}

public class ChatMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = string.Empty;
    public MessageRole Role { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public bool HasImage { get; set; }
    public byte[]? ImageData { get; set; }
    public string? ImageBase64 => ImageData != null ? Convert.ToBase64String(ImageData) : null;

    public bool IsUser => Role == MessageRole.User;
    public bool IsAssistant => Role == MessageRole.Assistant;

    public string TimeFormatted => Timestamp.ToString("HH:mm");
}
