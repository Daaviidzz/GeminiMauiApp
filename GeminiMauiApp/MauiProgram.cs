using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Media;
using GeminiMauiApp.Services;
using GeminiMauiApp.ViewModels;
using GeminiMauiApp.Views;
using Microsoft.Extensions.Logging;

namespace GeminiMauiApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // STT desde CommunityToolkit
            builder.Services.AddSingleton<ISpeechToText>(SpeechToText.Default);

            // Services
            builder.Services.AddHttpClient<GeminiService>();
            builder.Services.AddSingleton<GeminiService>();
            builder.Services.AddSingleton<SpeechService>();
            builder.Services.AddSingleton<SettingsService>();

            // ViewModels
            builder.Services.AddTransient<ChatViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();

            // Views
            builder.Services.AddTransient<ChatPage>();
            builder.Services.AddTransient<SettingsPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}