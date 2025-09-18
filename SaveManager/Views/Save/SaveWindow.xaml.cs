using SaveManager.Extensions;
using SaveManager.ViewModels;
using SaveManager.Views.GameProfile;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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


    private void SaveListBox_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Right)
        {
            HitTestResult r = VisualTreeHelper.HitTest(this, e.GetPosition(this));
            if (r.VisualHit.GetType() != typeof(ListBoxItem))
                SaveViewModel.SelectedEntry = null;
        }
    }


    private void SaveListItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            SaveViewModel.OpenCloseSelectedEntry();
        }        
    }


    private void AddFolderCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = false;
    }


    private void AddFolderCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        throw new NotImplementedException();
    }


    private void DeleteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = false;
    }


    private void DeleteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        throw new NotImplementedException();
    }


    private void RenameCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = false;
    }


    private void RenameCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        throw new NotImplementedException();
    }


    private void RefreshCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = false;
    }


    private void RefreshCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}
