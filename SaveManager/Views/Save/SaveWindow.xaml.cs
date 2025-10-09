using SaveManager.Assets;
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
    private const Key RenameKey = Key.F2;
    private const Key DeleteKey = Key.Delete;

    public SaveViewModel SaveViewModel { get; }

    public SaveWindow(SaveViewModel saveViewModel)
    {
        InitializeComponent();
        SaveViewModel = saveViewModel;
        DataContext = SaveViewModel;        
    }



    /// <summary>
    /// Returns a new dialog with the provided description for when an error occurs.
    /// </summary>
    /// <param name="description"></param>
    /// <returns></returns>
    private static OkDialog CreateErrorDialog(string description) => new("An error occurred", description, ImageSources.Error);

    /// <summary>
    /// Returns a new dialog for when the active game's savefile has not been set.
    /// </summary>
    private static OkDialog SaveNotSetDialog => new("Savefile location not set", 
        "The game's savefile location must be set to perform this action.", ImageSources.Warning);

    /// <summary>
    /// Returns a new dialog for when the active game's savefile does not exist.
    /// </summary>
    private static OkDialog SaveDoesNotExistDialog => new("Savefile does not exist", 
        "The game's savefile location does not exist.\nPlease set a new one.", ImageSources.Error);




    private void ProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SaveViewModel.EnableDisableButtons();
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


    private void SaveListBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        // used instead of binding to prevent flickering issues on context menu as much as possible
        AddFolderMenuItem.IsEnabled = SaveViewModel.CanAddFolder;
        DeleteMenuItem.IsEnabled = SaveViewModel.CanDelete;
        RenameMenuItem.IsEnabled = SaveViewModel.CanRename;
        RefreshMenuItem.IsEnabled = SaveViewModel.CanRefresh;
    }


    private void SaveListBox_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == RenameKey)
            PromptRenameSelectedEntry();
        else if (e.Key == DeleteKey)
            PromptDeleteSelectedEntry();
    }


    private void SaveListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SaveViewModel.EnableDisableButtons();
    }


    private void SaveListItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            SaveViewModel.OpenCloseSelectedEntry();
        }        
    }




    private void ImportSaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (SaveViewModel.ActiveGame == null || SaveViewModel.ActiveGame.ActiveProfile == null)
        {
            return;
        }

        if (SaveViewModel.ActiveGame.SavefileLocation == null)
        {
            SaveNotSetDialog.ShowDialog(this);
            return;
        }

        try
        {
            SaveViewModel.ImportSavefile();
        }
        catch (SavefileNotFoundException)
        {
            SaveDoesNotExistDialog.ShowDialog(this);
        }
        catch (FilesystemItemNotFoundException)
        {
            CreateErrorDialog("Reloading profiles from the filesystem...").ShowDialog(this);
            RefreshProfiles();
        }
        catch (FilesystemException)
        {
            CreateErrorDialog("An error occurred while backing up the savefile.").ShowDialog(this);
        }
    }




    private void LoadSaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (SaveViewModel.ActiveGame == null || SaveViewModel.ActiveGame.ActiveProfile == null || SaveViewModel.SelectedEntry is not File)
        {
            return;
        }

        if (SaveViewModel.ActiveGame.SavefileLocation == null)
        {
            SaveNotSetDialog.ShowDialog(this);
            return;
        }

        try
        {
            SaveViewModel.LoadSelectedEntry();
            ShowNotification(LoadedNotification);
        }
        catch (SavefileNotFoundException)
        {
            SaveDoesNotExistDialog.ShowDialog(this);
        }
        catch (FilesystemItemNotFoundException)
        {
            CreateErrorDialog("The savefile you are trying to load does not exist.\nReloading profiles from the filesystem...").ShowDialog(this);
            RefreshProfiles();
        }
        catch (FilesystemException)
        {
            CreateErrorDialog("An error occurred while loading the savefile.").ShowDialog(this);
        }
    }




    private void ReplaceSaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (SaveViewModel.ActiveGame == null || SaveViewModel.ActiveGame.ActiveProfile == null || SaveViewModel.SelectedEntry is not File)
        {
            return;
        }

        if (SaveViewModel.ActiveGame.SavefileLocation == null)
        {
            SaveNotSetDialog.ShowDialog(this);
            return;
        }

        try
        {
            SaveViewModel.ReplaceSelectedEntry();
            ShowNotification(ReplacedNotification);
        }
        catch (SavefileNotFoundException)
        {
            SaveDoesNotExistDialog.ShowDialog(this);
        }
        catch (FilesystemItemNotFoundException)
        {
            CreateErrorDialog("The savefile you are trying to replace does not exist.\nReloading profiles from the filesystem...").ShowDialog(this);
            RefreshProfiles();
        }
        catch (FilesystemException)
        {
            CreateErrorDialog("An error occurred while replacing the savefile.").ShowDialog(this);
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
                new OkDialog("Invalid name", ex.Message, ImageSources.Warning).ShowDialog(this);
                addFolderDialog = new(addFolderDialog.Title, addFolderDialog.Prompt, addFolderDialog.Input);
            }
            catch (FilesystemItemNotFoundException)
            {
                CreateErrorDialog("Reloading profiles from the filesystem...").ShowDialog(this);
                RefreshProfiles();
                return;
            }
            catch (FilesystemException)
            {
                CreateErrorDialog("An error occurred while creating a new folder.").ShowDialog(this);
                return;
            }
        }   
    }


    private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
    {
        PromptDeleteSelectedEntry();
    }


    private void RenameMenuItem_Click(object sender, RoutedEventArgs e)
    {
        PromptRenameSelectedEntry();
    }


    private void RefreshMenuItem_Click(object sender, RoutedEventArgs e)
    {
        RefreshProfiles();        
    }




    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        //SaveViewModel.SaveAppdata();
    }




    /// <summary>
    /// Asks the user if they want to delete the selected entry and then attempts to.
    /// </summary>
    private void PromptDeleteSelectedEntry()
    {
        if (!SaveViewModel.CanDelete)
            return;

        string message = SaveViewModel.SelectedEntry is File? $"Are you sure you want to delete '{SaveViewModel.SelectedEntry!.Name}'?":
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
                CreateErrorDialog($"'{SaveViewModel.SelectedEntry.Name}' does not exist.\nReloading profiles from the filesystem...").ShowDialog(this);
                RefreshProfiles();
            }
            catch (FilesystemException)
            {
                CreateErrorDialog($"An error occurred while deleting '{SaveViewModel.SelectedEntry.Name}'").ShowDialog(this);
            }
        }
    }


    /// <summary>
    /// Asks the user if they want to rename the selected entry and then attempts to.
    /// </summary>
    public void PromptRenameSelectedEntry()
    {
        if (!SaveViewModel.CanRename)
            return;

        InputDialog renameDialog = new($"Rename '{SaveViewModel.SelectedEntry!.Name}'", "Enter the new name:", SaveViewModel.SelectedEntry.Name);

        while (renameDialog.ShowDialog(this) == true)
        {
            try
            {
                SaveViewModel.RenameSelectedEntry(renameDialog.Input);
                return;
            }
            catch (ValidationException ex)
            {
                new OkDialog("Invalid name", ex.Message, ImageSources.Warning).ShowDialog(this);
                renameDialog = new(renameDialog.Title, renameDialog.Prompt, renameDialog.Input);
            }
            catch (FilesystemItemNotFoundException)
            {
                CreateErrorDialog($"'{SaveViewModel.SelectedEntry.Name}' does not exist.\nReloading profiles from the filesystem...").ShowDialog(this);
                RefreshProfiles();
                return;
            }
            catch (FilesystemException)
            {
                CreateErrorDialog($"An error occurred while renaming '{SaveViewModel.SelectedEntry.Name}'").ShowDialog(this);
                return;
            }
        }   
    }


    /// <summary>
    /// Attempts to reload the active game's profiles from the filesystem.
    /// </summary>
    private void RefreshProfiles()
    {
        if (!SaveViewModel.CanRefresh)
            return;

        if (SaveViewModel.ActiveGame!.ProfilesDirectory == null)
        {
            new OkDialog("Profiles directory not set", 
                "The game's profiles directory must be set to refresh profiles.", ImageSources.Warning).ShowDialog(this);
            return;
        }

        try
        {
            SaveViewModel.RefreshProfiles();
            ShowNotification(RefreshNotification);
        }
        catch (FilesystemException)
        {
            CreateErrorDialog("Failed to reload profiles.").ShowDialog(this);
        }
        catch (FilesystemItemNotFoundException)
        {
            new OkDialog("Profiles directory reset", 
                "The current game's profiles directory no longer exists.\nPlease set a new one.", ImageSources.Error).ShowDialog(this);
        }
    }


    /// <summary>
    /// Shows the provided notification on screen briefly.
    /// </summary>
    /// <param name="notification"></param>
    private void ShowNotification(FrameworkElement notification)
    {
        LoadedNotification.Visibility = Visibility.Hidden;
        ReplacedNotification.Visibility = Visibility.Hidden;
        RefreshNotification.Visibility = Visibility.Hidden;

        notification.Visibility = Visibility.Visible;
    }
}
