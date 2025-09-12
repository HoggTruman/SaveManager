using SaveManager.Extensions;
using SaveManager.ViewModels;
using SaveManager.Views.GameProfile;
using System.Windows;

namespace SaveManager.Views.Save;

/// <summary>
/// Interaction logic for SaveWindow.xaml
/// </summary>
public partial class SaveWindow : Window
{
    public SaveViewModel SaveViewModel { get; }

    public SaveWindow(SaveViewModel saveViewModel)
    {
        InitializeComponent();
        SaveViewModel = saveViewModel;
        DataContext = SaveViewModel;
    }

    private void GameProfileEditButton_Click(object sender, RoutedEventArgs e)
    {
        GameProfileWindow gameProfileWindow = new(ViewModelFactory.CreateGameProfileViewModel(SaveViewModel.Games, SaveViewModel.ActiveGame));
        gameProfileWindow.ShowDialog(this);
        SaveViewModel.Games = gameProfileWindow.GameProfileViewModel.Games;
        SaveViewModel.ActiveGame = gameProfileWindow.GameProfileViewModel.ActiveGame;
    }
}
