using SaveManager.Assets;
using SaveManager.Components;
using SaveManager.Exceptions;
using SaveManager.Extensions;
using SaveManager.Models;
using SaveManager.Services.Appdata;
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

    /// <summary>
    /// The position of mouse left click used to determine if the mouse has moved enough to start a drag.
    /// </summary>
    private Point _dragStartPoint = new();

    /// <summary>
    /// The filesystem item at the mouse left click. 
    /// Used so that when dragging at the edge of an item, the one orignally clicked is dragged, not the one dragged onto.
    /// </summary>
    private IFilesystemItem? _draggedItem;



    public SaveViewModel SaveViewModel { get; }

    public SaveWindow(SaveViewModel saveViewModel, StartupPreferences startupPreferences)
    {
        InitializeComponent();
        SaveViewModel = saveViewModel;
        DataContext = SaveViewModel;
        Width = startupPreferences.WindowWidth;
        Height = startupPreferences.WindowHeight;
        WindowState = startupPreferences.WindowMaximized ? WindowState.Maximized: WindowState.Normal;

        if (SaveViewModel.Games.Any(x => x.Name == startupPreferences.ActiveGame))
        {
            SaveViewModel.ActiveGame = SaveViewModel.Games.First(x => x.Name == startupPreferences.ActiveGame);
            SaveViewModel.ActiveGame.TrySetActiveProfileByName(startupPreferences.ActiveProfile);
        }
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




    private void GameEditButton_Click(object sender, RoutedEventArgs e)
    {
        GameProfileWindow gameProfileWindow = new(ViewModelFactory.CreateGameProfileViewModel(SaveViewModel.Games, SaveViewModel.ActiveGame));
        gameProfileWindow.ShowDialog(this);
        SaveViewModel.Games = [..gameProfileWindow.GameProfileViewModel.Games];
        SaveViewModel.ActiveGame = gameProfileWindow.GameProfileViewModel.ActiveGame;
        SaveViewModel.SelectedEntry = null;
    }




    private void CreateProfileButton_Click(object sender, RoutedEventArgs e)
    {
        if (SaveViewModel.ActiveGame == null)
        {
            return;
        }

        if (SaveViewModel.ActiveGame.ProfilesDirectory == null)
        {
            new OkDialog("Profiles directory not set", 
                "The game's profiles directory must be set before you can create a profile.", ImageSources.Warning).ShowDialog(this);
            return;
        }

        InputDialog newProfileDialog = new("New Profile", "Enter the name of the new profile:");

        while (newProfileDialog.ShowDialog(this) == true)
        {
            try
            {
                SaveViewModel.CreateProfile(newProfileDialog.Input);
                return;
            }
            catch (ValidationException ex)
            {
                new OkDialog("Invalid name", ex.Message, ImageSources.Warning).ShowDialog(this);
                newProfileDialog = new(newProfileDialog.Title, newProfileDialog.Prompt, newProfileDialog.Input);
            }
            catch (FilesystemMismatchException)
            {
                CreateErrorDialog("An error occurred while creating a new profile.\nReloading profiles from the filesystem...").ShowDialog(this);
                RefreshProfiles();
                return;
            }
            catch (FilesystemException)
            {
                CreateErrorDialog("An error occurred while creating a new profile.").ShowDialog(this);
                return;
            }
        }     
    }


    private void RenameProfileButton_Click(object sender, RoutedEventArgs e)
    {
        if (SaveViewModel.ActiveGame == null || SaveViewModel.ActiveGame.ActiveProfile == null)
        {
            return;
        }

        InputDialog renameProfileDialog = new("Rename Profile", "Enter the new name of the profile:", SaveViewModel.ActiveGame.ActiveProfile.Name);

        while (renameProfileDialog.ShowDialog(this) == true)
        {
            try
            {
                SaveViewModel.RenameProfile(renameProfileDialog.Input);
                return;
            }
            catch (ValidationException ex)
            {
                new OkDialog("Invalid name", ex.Message, ImageSources.Warning).ShowDialog(this);
                renameProfileDialog = new(renameProfileDialog.Title, renameProfileDialog.Prompt, renameProfileDialog.Input);
            }
            catch (FilesystemMismatchException)
            {
                CreateErrorDialog("The profile you are trying to rename does not exist.\nReloading profiles from the filesystem...").ShowDialog(this);
                RefreshProfiles();
                return;
            }
            catch (FilesystemException)
            {
                CreateErrorDialog("An error occurred while renaming the profile.").ShowDialog(this);
                return;
            }
        }
    }


    private void DeleteProfileButton_Click(object sender, RoutedEventArgs e)
    {
        if (SaveViewModel.ActiveGame == null || SaveViewModel.ActiveGame.ActiveProfile == null)
        {
            return;
        }

        YesNoDialog confirmationDialog = new("Delete Profile", 
            "Are you sure you want to delete this profile?\nThis will delete the associated folder and all its contents.");

        if (confirmationDialog.ShowDialog(this) == true)
        {
            try
            {
                SaveViewModel.DeleteProfile();
            }
            catch (FilesystemMismatchException)
            {
                CreateErrorDialog("The profile you are trying to delete does not exist.\nReloading profiles from the filesystem...").ShowDialog(this);
                RefreshProfiles();
            }
            catch (FilesystemException)
            {
                CreateErrorDialog("An error occurred while deleting the profile.").ShowDialog(this);
            }
        }
    }    




    private void SaveListBox_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Right)
        {
            SaveViewModel.SelectedEntry = null;
        }
    }


    private void SaveListBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        // updates selection when right click during a drag
        SaveViewModel.SelectedEntry = ((FrameworkElement)e.OriginalSource).DataContext as IFilesystemItem;

        // used instead of binding to prevent flickering issues on context menu as much as possible
        AddFolderMenuItem.IsEnabled = SaveViewModel.CanAddFolder;
        DeleteMenuItem.IsEnabled = SaveViewModel.CanDelete;
        RenameMenuItem.IsEnabled = SaveViewModel.CanRename;
        RefreshMenuItem.IsEnabled = SaveViewModel.CanRefresh;
    }


    private void SaveListBox_Drop(object sender, DragEventArgs e)
    {
        IFilesystemItem draggedItem = (IFilesystemItem)e.Data.GetData(typeof(IFilesystemItem));       
        MoveEntry(draggedItem, null);
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
            e.Handled = true;
        }        
    }


    private void SaveListItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        // preview used to prevent the default ListBoxItem MouseDown blocking the event.
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            _dragStartPoint = e.GetPosition(this);
            _draggedItem = (IFilesystemItem)((FrameworkElement)sender).DataContext;
        }
    }


    private void SaveListItem_MouseMove(object sender, MouseEventArgs e)
    {
        if (_draggedItem != null && e.LeftButton == MouseButtonState.Pressed)
        {
            Point current = e.GetPosition(this);
            if (Math.Abs(current.X - _dragStartPoint.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(current.Y - _dragStartPoint.Y) >= SystemParameters.MinimumVerticalDragDistance)
            {
                SaveViewModel.SelectedEntry = _draggedItem;
                SetDropTargets();
                DataObject data = new(typeof(IFilesystemItem), _draggedItem);
                DragDrop.DoDragDrop((FrameworkElement)sender, data, DragDropEffects.Move);
                _draggedItem = null;
            }
        }
    }


    private void SaveListItem_Drop(object sender, DragEventArgs e)
    {
        IFilesystemItem draggedItem = (IFilesystemItem)e.Data.GetData(typeof(IFilesystemItem));
        IFilesystemItem? droppedOnItem = (IFilesystemItem)((FrameworkElement)sender).DataContext;
        SetListBoxItemDraggingStyle(sender, false);
        MoveEntry(draggedItem, droppedOnItem);        
        // set handled to true to prevent SaveListBox_Drop being called too.
        e.Handled = true;
    }


    private void SaveListItem_DragEnter(object sender, DragEventArgs e)
    {
        SetListBoxItemDraggingStyle(sender, true);
    }


    private void SaveListItem_DragLeave(object sender, DragEventArgs e)
    {
        SetListBoxItemDraggingStyle(sender, false);
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
        catch (FilesystemMismatchException)
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
        catch (FilesystemMismatchException)
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
        catch (FilesystemMismatchException)
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
            catch (FilesystemMismatchException)
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


    private void RenameMenuItem_Click(object sender, RoutedEventArgs e)
    {
        PromptRenameSelectedEntry();
    }


    private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
    {
        PromptDeleteSelectedEntry();
    }    


    private void RefreshMenuItem_Click(object sender, RoutedEventArgs e)
    {
        RefreshProfiles();        
    }




    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        // resets the dragged item when clicking elsewhere to prevent initiating drags from 
        // outside the savelist entry
        _draggedItem = null;
    }


    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        StartupPreferences startupPreferences = new()
        {
            ActiveGame = SaveViewModel.ActiveGame?.Name,
            ActiveProfile = SaveViewModel.ActiveGame?.ActiveProfile?.Name,
            WindowWidth = Width,
            WindowHeight = Height,
            WindowMaximized = WindowState == WindowState.Maximized
        };

        SaveViewModel.SaveAppdata(startupPreferences);
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
            catch (FilesystemMismatchException)
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
            catch (FilesystemMismatchException)
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
    /// Moves an entry to a different folder based on where it is dropped.
    /// </summary>
    /// <param name="draggedItem"></param>
    /// <param name="droppedOnItem"></param>
    private void MoveEntry(IFilesystemItem draggedItem, IFilesystemItem? droppedOnItem)
    {
        if (SaveViewModel.ActiveGame == null || SaveViewModel.ActiveGame.ActiveProfile == null || draggedItem == droppedOnItem)
        {
            return;
        }

        try
        {
            SaveViewModel.MoveEntry(draggedItem, droppedOnItem);
        }
        catch (ValidationException ex)
        {
            new OkDialog($"{(draggedItem is File ? "File" : "Folder")} already exists", ex.Message, ImageSources.Warning).ShowDialog(this);
        }
        catch (FilesystemMismatchException)
        {
            CreateErrorDialog("Reloading profiles from the filesystem...").ShowDialog(this);
            RefreshProfiles();
        }
        catch (FilesystemException)
        {
            CreateErrorDialog($"An error occurred while moving the {(draggedItem is File ? "File" : "Folder")}.").ShowDialog(this);
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
        catch (FilesystemMismatchException)
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


    /// <summary>
    /// Sets the "AllowDrop" value for each <see cref="ListBoxItem"/> based on the dragged item.
    /// </summary>
    private void SetDropTargets()
    {
        if (_draggedItem == null)
        {
            throw new InvalidOperationException("There must be a dragged item to set drop targets");
        }

        foreach (IFilesystemItem item in SaveListBox.Items)
        {
            FrameworkElement child = (FrameworkElement)SaveListBox.ItemContainerGenerator.ContainerFromItem(item);
            child.AllowDrop = SaveViewModel.IsValidDropTarget(_draggedItem, item);
        }
    }

    /// <summary>
    /// Updates the style for a ListBoxItem being dragged over.
    /// </summary>
    /// <param name="sender">The object that triggered the drag related event</param>
    /// <param name="isDraggedOver">true if the sender currently being dragged over. Otherwise false.</param>
    private void SetListBoxItemDraggingStyle(object sender, bool isDraggedOver)
    {
        object item = ((FrameworkElement)sender).DataContext;
        ListBoxItem listBoxItem = (ListBoxItem)SaveListBox.ItemContainerGenerator.ContainerFromItem(item);
        listBoxItem.BorderBrush = isDraggedOver ? (DrawingBrush)Resources["DraggedOverBorderBrush"] : null;
    }
}
