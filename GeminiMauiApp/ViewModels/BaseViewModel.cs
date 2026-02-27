using CommunityToolkit.Mvvm.ComponentModel;

namespace GeminiMauiApp.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    [ObservableProperty]
    private string _title = string.Empty;

    public bool IsNotBusy => !IsBusy;

    protected async Task SafeExecuteAsync(Func<Task> action, string? errorPrefix = null)
    {
        try
        {
            IsBusy = true;
            await action();
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            var msg = errorPrefix != null ? $"{errorPrefix}: {ex.Message}" : ex.Message;
            await Shell.Current.DisplayAlert("Error", msg, "Cerrar");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
