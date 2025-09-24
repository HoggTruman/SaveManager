using SaveManager.Components;
using SaveManager.Exceptions;
using SaveManager.Extensions;
using SaveManager.Models;
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
        if (!SaveViewModel.CanAddFolder)
            return;

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
                new OkDialog("An error occurred", "An error occurred while creating a new folder.\nReloading profiles from the filesystem...").ShowDialog(this);
                RefreshProfiles();
                return;
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
        DeleteSelectedEntry();
    }


    private void RenameMenuItem_Click(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }


    private void RefreshMenuItem_Click(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }




    private void DeleteSelectedEntry()
    {
        if (!SaveViewModel.CanDelete)
            return;

        bool isFile = SaveViewModel.SelectedEntry is File;
        string message = isFile? $"Are you sure you want to delete '{SaveViewModel.SelectedEntry!.Name}'?":
            $"Are you sure you want to delete '{SaveViewModel.SelectedEntry!.Name}' and all its contents?";

        YesNoDialog confirmDeleteDialog = new($"Delete '{SaveViewModel.SelectedEntry!.Name}'", message);

        if (confirmDeleteDialog.ShowDialog(this) == true)
        {
            try
            {
                SaveViewModel.DeleteSelectedEntry();
            }
            catch (FilesystemItemNotFoundException)
            {
                new OkDialog($"{(isFile? "File": "Folder")} does not exist", 
                    $"'{SaveViewModel.SelectedEntry.Name}' no longer exists.\nReloading profiles from the filesystem...").ShowDialog(this);
                RefreshProfiles();
            }
            catch (FilesystemException)
            {
                new OkDialog("An error occurred", $"An error occurred while deleting the {(isFile? "file": "folder")}.").ShowDialog(this);
            }
        }
    }


    private void RefreshProfiles()
    {
        if (SaveViewModel.ActiveGame == null)
            return;

        try
        {
            SaveViewModel.RefreshProfiles();
            new OkDialog("Profiles reloaded", "The current game's profiles have been reloaded.").ShowDialog(this);
        }
        catch (FilesystemException)
        {
            new OkDialog("An error occurred", "Failed to reload profiles.").ShowDialog(this);
        }
        catch (FilesystemItemNotFoundException)
        {
            new OkDialog("Profiles directory reset", "The current game's profiles directory no longer exists.\nPlease set a new one.").ShowDialog(this);
        }
    }


    private void SaveListBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        // used instead of binding to prevent flickering issues on context menu as much as possible
        AddFolderMenuItem.IsEnabled = SaveViewModel.CanAddFolder;
        DeleteMenuItem.IsEnabled = SaveViewModel.CanDelete;
        RenameMenuItem.IsEnabled = SaveViewModel.CanRename;
        RefreshMenuItem.IsEnabled = SaveViewModel.CanRefresh;
    }
}
