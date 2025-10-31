using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SaveManager.ViewModels;

public abstract class NotifyPropertyChanged : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName=null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName=null)
    {
        field = value;
        OnPropertyChanged(propertyName);
    }
}
