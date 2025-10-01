using Microsoft.WindowsAPICodePack.Dialogs;
using SaveManager.Components;
using SaveManager.Exceptions;
using SaveManager.Extensions;
using SaveManager.ViewModels;
using System.IO;
using System.Windows;

namespace SaveManager.Views.GameProfile;

/// <summary>
/// Interaction logic for GameProfileWindow.xaml
/// </summary>
public partial class GameProfileWindow : Window
{
    public GameProfileViewModel GameProfileViewModel { get; }

    public GameProfileWindow(GameProfileViewModel gameProfileViewModel)
    {
        InitializeComponent();
        GameProfileViewModel = gameProfileViewModel;
        DataContext = GameProfileViewModel;
    }


    private void AddGameButton_Click(object sender, RoutedEventArgs e)
    {   
        InputDialog addGameDialog = new("Add Game", "Enter the name of the new game:");

        while (addGameDialog.ShowDialog(this) == true)
        {
            try
            {
                GameProfileViewModel.AddGame(addGameDialog.Input);
                break;
            }
            catch (ValidationException ex)
            {
                new OkDialog("Invalid name", ex.Message).ShowDialog(this);
                addGameDialog = new(addGameDialog.Title, addGameDialog.Prompt, addGameDialog.Input);
            }                                  
        }            
    }


    private void RemoveGameButton_Click(object sender, RoutedEventArgs e)
    {
        if (GameProfileViewModel.ActiveGame == null)
        {
            return;
        }

        YesNoDialog confirmationDialog = new("Remove Game",
            $"Are you sure you want to remove '{GameProfileViewModel.ActiveGame.Name}'?\nIts profiles directory will not be deleted.");

        if (confirmationDialog.ShowDialog(this) == true)
        {
            GameProfileViewModel.RemoveGame();
        }
    }


    private void RenameGameButton_Click(object sender, RoutedEventArgs e)
    {
        if (GameProfileViewModel.ActiveGame == null)
        {
            return;
        }

        InputDialog renameGameDialog = new("Rename Game", "Enter the new name of the game:", GameProfileViewModel.ActiveGame.Name);

        while (renameGameDialog.ShowDialog(this) == true)
        {
            try
            {
                GameProfileViewModel.RenameGame(renameGameDialog.Input);
                break;
            }
            catch (ValidationException ex)
            {
                new OkDialog("Invalid name", ex.Message).ShowDialog(this);
                renameGameDialog = new(renameGameDialog.Title, renameGameDialog.Prompt, renameGameDialog.Input);
            }                                  
        }   
    }


    private void SavefileBrowseButton_Click(object sender, RoutedEventArgs e)
    {
        if (GameProfileViewModel.ActiveGame == null)
        {
            return;
        }

        CommonOpenFileDialog openFileDialog = new()
        {
            IsFolderPicker = false,
            Title = "Select a savefile",
            InitialDirectory = Path.GetDirectoryName(GameProfileViewModel.ActiveGame.SavefileLocation)            
        };

        if (openFileDialog.ShowDialog(this) == CommonFileDialogResult.Ok)
        {
            try
            {
                GameProfileViewModel.SetSavefileLocation(openFileDialog.FileName);
            }
            catch (ValidationException ex)
            {
                new OkDialog("Invalid file", ex.Message).ShowDialog(this);
            }
            catch (FilesystemException)
            {
                new OkDialog("An error occurred", "An error occurred while setting the savefile location.").ShowDialog(this);
            }
        }
    }   


    private void ProfilesDirectoryBrowseButton_Click(object sender, RoutedEventArgs e)
    {
        if (GameProfileViewModel.ActiveGame == null)
        {
            return;
        }

        CommonOpenFileDialog openFolderDialog = new()
        {
            IsFolderPicker = true,
            Title = "Select a profiles directory",
            InitialDirectory = GameProfileViewModel.ActiveGame?.ProfilesDirectory
        };

        if (openFolderDialog.ShowDialog(this) == CommonFileDialogResult.Ok)
        {
            try
            {
                GameProfileViewModel.SetProfilesDirectory(openFolderDialog.FileName);
            }
            catch (ValidationException ex)
            {
                new OkDialog("Invalid directory", ex.Message).ShowDialog(this);
            }
            catch (FilesystemException)
            {
                new OkDialog("An error occurred", "An error occurred while setting the profiles directory.").ShowDialog(this);
            }
        }
    }


    private void NewProfileButton_Click(object sender, RoutedEventArgs e)
    {
        if (GameProfileViewModel.ActiveGame == null || GameProfileViewModel.ActiveGame.ProfilesDirectory == null)
        {
            return;
        }

        InputDialog newProfileDialog = new("New Profile", "Enter the name of the new profile:");

        while (newProfileDialog.ShowDialog(this) == true)
        {
            try
            {
                GameProfileViewModel.CreateProfile(newProfileDialog.Input);
                return;
            }
            catch (ValidationException ex)
            {
                new OkDialog("Invalid name", ex.Message).ShowDialog(this);
                newProfileDialog = new(newProfileDialog.Title, newProfileDialog.Prompt, newProfileDialog.Input);
            }
            catch (FilesystemItemNotFoundException)
            {
                new OkDialog("An error occurred", "An error occurred while creating a new profile.\nReloading profiles from the filesystem...").ShowDialog(this);
                RefreshProfiles();
                return;
            }
            catch (FilesystemException)
            {
                new OkDialog("An error occurred", "An error occurred while creating a new profile.").ShowDialog(this);
                return;
            }
        }     
    }


    private void RenameProfileButton_Click(object sender, RoutedEventArgs e)
    {
        if (GameProfileViewModel.SelectedProfile == null)
        {
            return;
        }

        InputDialog renameProfileDialog = new("Rename Profile", "Enter the new name of the profile:", GameProfileViewModel.SelectedProfile.Name);

        while (renameProfileDialog.ShowDialog(this) == true)
        {
            try
            {
                GameProfileViewModel.RenameProfile(renameProfileDialog.Input);
                return;
            }
            catch (ValidationException ex)
            {
                new OkDialog("Invalid name", ex.Message).ShowDialog(this);
                renameProfileDialog = new(renameProfileDialog.Title, renameProfileDialog.Prompt, renameProfileDialog.Input);
            }
            catch (FilesystemItemNotFoundException)
            {
                new OkDialog("An error occurred", "The profile you are trying to rename does not exist.\nReloading profiles from the filesystem...").ShowDialog(this);
                RefreshProfiles();
                return;
            }
            catch (FilesystemException)
            {
                new OkDialog("An error occurred", "An error occurred while renaming the profile.").ShowDialog(this);
                return;
            }
        }   
    }


    private void DeleteProfileButton_Click(object sender, RoutedEventArgs e)
    {
        if (GameProfileViewModel.SelectedProfile == null)
        {
            return;
        }

        YesNoDialog confirmationDialog = new("Delete Profile", "Are you sure you want to delete this profile?\nThis will delete the associated folder and all its contents.");

        if (confirmationDialog.ShowDialog(this) == true)
        {
            try
            {
                GameProfileViewModel.DeleteProfile();
            }
            catch (FilesystemItemNotFoundException)
            {
                new OkDialog("An error occurred", "The profile you are trying to delete does not exist.\nReloading profiles from the filesystem...").ShowDialog(this);
                RefreshProfiles();
            }
            catch (FilesystemException)
            {
                new OkDialog("An error occurred", "An error occurred while deleting the profile.").ShowDialog(this);
            }
        }
    }


    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        bool shouldAttemptSave = true;

        while (shouldAttemptSave)
        {      
            try
            {
                GameProfileViewModel.SaveGameChanges();
                shouldAttemptSave = false;
            }
            catch (AppdataException)
            {
                YesNoDialog tryAgainDialog = new("Failed to save",
                    "Failed to save changes to games (appdata.xml might be in use by another program)\nWould you like to try again?");

                if (tryAgainDialog.ShowDialog(this) != true)
                {
                    e.Cancel = true;
                    shouldAttemptSave = false;
                }
            }
        }
    }


    private void RefreshProfiles()
    {
        if (GameProfileViewModel.ActiveGame == null)
            return;

        try
        {
            GameProfileViewModel.RefreshProfiles();
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
}
