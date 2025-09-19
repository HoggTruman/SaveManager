using Microsoft.WindowsAPICodePack.Dialogs;
using SaveManager.Components;
using SaveManager.Exceptions;
using SaveManager.Extensions;
using SaveManager.ViewModels;
using System;
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
            GameProfileViewModel.ActiveGame.SavefileLocation = openFileDialog.FileName;       
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

        while (openFolderDialog.ShowDialog(this) == CommonFileDialogResult.Ok)
        {
            try
            {
                GameProfileViewModel.SetProfilesDirectory(openFolderDialog.FileName);
                return;
            }
            catch (ValidationException ex)
            {
                new OkDialog("Invalid Directory", ex.Message).ShowDialog(this);
            }
            catch (Exception ex)
            {
                if (ex is FilesystemException or FilesystemItemNotFoundException)
                {
                    new OkDialog("An error occurred", "An error occurred while setting the profiles directory.").ShowDialog(this);
                    return;
                }

                throw;
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
                GameProfileViewModel.HandleFilesystemItemNotFound();
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
                GameProfileViewModel.HandleFilesystemItemNotFound();
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
                GameProfileViewModel.HandleFilesystemItemNotFound();
                return;
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


    private void HandleFilesystemItemNotFoundException(FilesystemException exception)
    {
        if (GameProfileViewModel.ActiveGame == null)
            return;

        try
        {
            GameProfileViewModel.HandleFilesystemItemNotFound();

            if (GameProfileViewModel.ActiveGame.ProfilesDirectory == null)
            {
                new OkDialog("Profiles directory reset", 
                    "The current game's profiles directory no longer exists and has been reset.\nPlease set a new one.").ShowDialog(this);
            }
            else
            {
                new OkDialog("Profiles reloaded", "The selected profile could not be found.\nThe current game's profiles have been reloaded.").ShowDialog(this);
            }
        }
        catch (FilesystemException ex)
        {
            new OkDialog("An error occurred", "An error occurred.\nAttempting to reload profiles.").ShowDialog(this);
            HandleFilesystemItemNotFoundException(ex);
        }           
    }
}
