using GeminiMauiApp.ViewModels;

namespace GeminiMauiApp.Views;

public partial class ChatPage : ContentPage
{
    private readonly ChatViewModel _vm;

    public ChatPage(ChatViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;

        _vm.Messages.CollectionChanged += async (_, _) =>
        {
            await Task.Delay(100); 
            if (_vm.Messages.Count > 0)
                ChatList.ScrollTo(_vm.Messages[^1], ScrollToPosition.End, animate: true);
        };
    }
}
