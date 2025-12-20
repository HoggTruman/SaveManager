using SaveManager.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace SaveManager.Views.Settings;

/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow : Window
{
    public SettingsViewModel SettingsViewModel { get; }

    public SettingsWindow(SettingsViewModel settingsViewModel)
    {
        InitializeComponent();
        SettingsViewModel = settingsViewModel;
    }


    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        RootGrid.Focus();
    }


    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {

    }


    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {

    }
}
