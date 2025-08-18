using SaveManager.Services;
using SaveManager.ViewModels;
using SaveManager.Views.GameProfile;
using System.Windows;

namespace SaveManager.Views.Save;

/// <summary>
/// Interaction logic for SaveWindow.xaml
/// </summary>
public partial class SaveWindow : Window
{
    public SaveViewModel SaveViewModel { get; } = new(new AppdataService());

    public SaveWindow()
    {
        InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        GameProfileWindow gameProfileWindow = new();
        gameProfileWindow.ShowDialog();
    }
}
