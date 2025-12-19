using SaveManager.Services.Hotkey;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SaveManager.Views.Settings;

/// <summary>
/// Interaction logic for HotkeyInputControl.xaml
/// </summary>
public partial class HotkeyInputControl : UserControl, INotifyPropertyChanged
{
    private string _display = "";


    public event PropertyChangedEventHandler? PropertyChanged;


    public static readonly DependencyProperty HotkeyProperty = DependencyProperty.Register(
        nameof(Hotkey),
        typeof(Hotkey),
        typeof(HotkeyInputControl),
        new FrameworkPropertyMetadata(
            default(Hotkey),
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );
    

    public Hotkey? Hotkey
    {
        get => (Hotkey)GetValue(HotkeyProperty);
        set
        {
            SetValue(HotkeyProperty, value);
            Display = value == null? "": value.ToString();
        }
    }

    public string Display
    {
        get => _display;
        set
        {
            _display = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Display)));
        }
    }




    public HotkeyInputControl()
    {
        InitializeComponent();
    }


    private void HotkeyTextbox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;

        Key key = e.Key == Key.System ? e.SystemKey : e.Key;

        if (key == Key.LeftShift || key == Key.RightShift || 
            key == Key.LeftCtrl || key == Key.RightCtrl ||
            key == Key.LeftAlt || key == Key.RightAlt || 
            key == Key.LWin || key == Key.RWin) 
        {
            return;
        }

        Hotkey = new() { Key = key, ModifierKeys = Keyboard.Modifiers };
    }


    private void HotkeyTextbox_GotFocus(object sender, RoutedEventArgs e)
    {
        Display = "...";
    }


    private void HotkeyTextbox_LostFocus(object sender, RoutedEventArgs e)
    {
        Display = Hotkey == null? "": Hotkey.ToString();
    }
}
