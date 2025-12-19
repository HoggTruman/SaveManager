using SaveManager.Services.Hotkey;
using System.Windows;
using System.Windows.Input;

namespace SaveManager.Views.Settings;

/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        RootGrid.Focus();
    }
}
