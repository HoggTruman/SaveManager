using SaveManager.Components;
using SaveManager.Exceptions;
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
        SaveViewModel.Games = [..gameProfileWindow.GameProfileViewModel.Games];
        SaveViewModel.ActiveGame = gameProfileWindow.GameProfileViewModel.ActiveGame;
        SaveViewModel.SelectedEntry = null;
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
        

    private void AddFolderMenuItem_Click(object sender, RoutedEventArgs e)
    {
        InputDialog addFolderDialog = new("Add Folder", "Enter the name of the new folder:");

        while (addFolderDialog.ShowDialog(this) == true)
        {
            try
            {
                SaveViewModel.AddFolder(addFolderDialog.Input);
                return;
            }
            catch (ValidationException ex)
            {
                new OkDialog("Invalid name", ex.Message).ShowDialog(this);
                addFolderDialog = new(addFolderDialog.Title, addFolderDialog.Prompt, addFolderDialog.Input);
            }
            catch (FilesystemItemNotFoundException)
            {
                HandleFilesystemItemNotFound();
            }
            catch (FilesystemException)
            {
                new OkDialog("An error occurred", "An error occurred while creating a new folder.").ShowDialog(this);
                return;
            }
        }   
    }


    private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void RenameMenuItem_Click(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void RefreshMenuItem_Click(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }


    private void HandleFilesystemItemNotFound()
    {
        if (SaveViewModel.ActiveGame == null)
            return;

        try
        {
            SaveViewModel.HandleFilesystemItemNotFound();

            if (SaveViewModel.ActiveGame.ProfilesDirectory == null)
            {
                new OkDialog("Profiles directory reset", 
                    "The current game's profiles directory no longer exists and has been reset.\nPlease set a new one.").ShowDialog(this);
            }
            else
            {
                new OkDialog("Profiles reloaded", "The selected profile could not be found.\nThe current game's profiles have been reloaded.").ShowDialog(this);
            }
        }
        catch (FilesystemException)
        {
            new OkDialog("An error occurred", "An error occurred.\nAttempting to reload profiles.").ShowDialog(this);
            HandleFilesystemItemNotFound();
        }
    }
}
