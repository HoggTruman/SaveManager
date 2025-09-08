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
    public readonly GameProfileViewModel _gameProfileViewModel;

    public GameProfileWindow(GameProfileViewModel gameProfileViewModel)
    {
        InitializeComponent();
        _gameProfileViewModel = gameProfileViewModel;
        DataContext = _gameProfileViewModel;
    }


    private void AddGameButton_Click(object sender, RoutedEventArgs e)
    {   
        InputDialog addGameDialog = new("Add Game", "Enter the name of the new game:");

        while (addGameDialog.ShowDialog(this) == true)
        {
            try
            {
                _gameProfileViewModel.AddGame(addGameDialog.Input);
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
        if (_gameProfileViewModel.ActiveGame == null)
        {
            return;
        }

        YesNoDialog confirmationDialog = new("Remove Game",
            $"Are you sure you want to remove '{_gameProfileViewModel.ActiveGame.Name}'?\nIts profiles directory will not be deleted.");

        if (confirmationDialog.ShowDialog(this) == true)
        {
            _gameProfileViewModel.RemoveGame();
        }
    }


    private void RenameGameButton_Click(object sender, RoutedEventArgs e)
    {
        if (_gameProfileViewModel.ActiveGame == null)
        {
            return;
        }

        InputDialog renameGameDialog = new("Rename Game", "Enter the new name of the game:", _gameProfileViewModel.ActiveGame.Name);

        while (renameGameDialog.ShowDialog(this) == true)
        {
            try
            {
                _gameProfileViewModel.RenameGame(renameGameDialog.Input);
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
        if (_gameProfileViewModel.ActiveGame == null)
        {
            return;
        }

        CommonOpenFileDialog openFileDialog = new()
        {
            IsFolderPicker = false,
            Title = "Select a savefile",
            InitialDirectory = Path.GetDirectoryName(_gameProfileViewModel.ActiveGame.SavefileLocation)            
        };

        if (openFileDialog.ShowDialog(this) == CommonFileDialogResult.Ok)
        {
            _gameProfileViewModel.ActiveGame!.SavefileLocation = openFileDialog.FileName;       
        }
    }   


    private void ProfilesDirectoryBrowseButton_Click(object sender, RoutedEventArgs e)
    {
        if (_gameProfileViewModel.ActiveGame == null)
        {
            return;
        }

        CommonOpenFileDialog openFolderDialog = new()
        {
            IsFolderPicker = true,
            Title = "Select a profiles directory",
            InitialDirectory = _gameProfileViewModel.ActiveGame?.ProfilesDirectory
        };

        while (openFolderDialog.ShowDialog(this) == CommonFileDialogResult.Ok)
        {
            try
            {
                _gameProfileViewModel.SetProfilesDirectory(openFolderDialog.FileName);
                break;
            }
            catch (ValidationException ex)
            {
                new OkDialog("Invalid Directory", ex.Message).ShowDialog(this);
            }
        }
    }


    private void NewProfileButton_Click(object sender, RoutedEventArgs e)
    {
        if (_gameProfileViewModel.ActiveGame == null)
        {
            return;
        }

        InputDialog newProfileDialog = new("New Profile", "Enter the name of the new profile:");

        while (newProfileDialog.ShowDialog(this) == true)
        {
            try
            {
                _gameProfileViewModel.CreateProfile(newProfileDialog.Input);
                break;
            }
            catch (ValidationException ex)
            {
                new OkDialog("Invalid name", ex.Message).ShowDialog(this);
                newProfileDialog = new(newProfileDialog.Title, newProfileDialog.Prompt, newProfileDialog.Input);
            }
            catch (FilesystemException ex)
            {
                new OkDialog("An error occurred", "An error occurred while creating a new profile.").ShowDialog(this);
                HandleFilesystemException(ex);
                break;
            }
        }     
    }


    private void RenameProfileButton_Click(object sender, RoutedEventArgs e)
    {
        if (_gameProfileViewModel.SelectedProfile == null)
        {
            return;
        }

        InputDialog renameProfileDialog = new("Rename Profile", "Enter the new name of the profile:", _gameProfileViewModel.SelectedProfile.Name);

        while (renameProfileDialog.ShowDialog(this) == true)
        {
            try
            {
                _gameProfileViewModel.RenameProfile(renameProfileDialog.Input);
                break;
            }
            catch (ValidationException ex)
            {
                new OkDialog("Invalid name", ex.Message).ShowDialog(this);
                renameProfileDialog = new(renameProfileDialog.Title, renameProfileDialog.Prompt, renameProfileDialog.Input);
            }
            catch (FilesystemException ex)
            {
                new OkDialog("An error occurred", "An error occurred while renaming the profile.").ShowDialog(this);
                HandleFilesystemException(ex);
                break;
            }
        }   
    }


    private void DeleteProfileButton_Click(object sender, RoutedEventArgs e)
    {
        if (_gameProfileViewModel.SelectedProfile == null)
        {
            return;
        }

        YesNoDialog confirmationDialog = new("Delete Profile", "Are you sure you want to delete this profile?\nThis will delete the associated folder and all its contents.");

        if (confirmationDialog.ShowDialog() == true)
        {
            try
            {
                _gameProfileViewModel.DeleteProfile();
            }
            catch (FilesystemException ex)
            {
                new OkDialog("An error occurred", "An error occurred while deleting the profile.").ShowDialog(this);
                HandleFilesystemException(ex);
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
                _gameProfileViewModel.SaveGameChanges();
                shouldAttemptSave = false;
            }
            catch (AppdataException)
            {
                YesNoDialog tryAgainDialog = new("Failed to save",
                    "Failed to save changes to games (appdata.xml might be being accessed by another program)\nWould you like to try again?");

                if (tryAgainDialog.ShowDialog(this) != true)
                {
                    YesNoDialog exitWithoutSavingDialog = new("Exit without saving?",
                        "Exit without saving changes to games?");

                    if (exitWithoutSavingDialog.ShowDialog(this) != true)
                    {
                        e.Cancel = true;                        
                    }

                    shouldAttemptSave = false;
                }
            }
        }
    }


    private void HandleFilesystemException(FilesystemException exception)
    {
        Console.Write(exception);
        try
        {
            _gameProfileViewModel.RefreshGame();
        }
        catch (FilesystemException ex)
        {
            new OkDialog("An error occurred", "An error occurred while attempting to reload the game.").ShowDialog(this);
            HandleFilesystemException(ex);
        }           
    }
}
