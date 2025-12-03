using Microsoft.WindowsAPICodePack.Dialogs;
using SaveManager.Assets;
using SaveManager.Components.Dialogs;
using SaveManager.Exceptions;
using SaveManager.Helpers;
using SaveManager.ViewModels;
using System.IO;
using System.Windows;

namespace SaveManager.Views.GameEdit;

/// <summary>
/// Interaction logic for GameEditWindow.xaml
/// </summary>
public partial class GameEditWindow : Window
{
    public GameEditViewModel GameEditViewModel { get; }

    public GameEditWindow(GameEditViewModel gameEditViewModel)
    {
        InitializeComponent();
        GameEditViewModel = gameEditViewModel;
        DataContext = GameEditViewModel;
    }



    
    private static OkDialog CreateErrorDialog(string description) => new("An error occurred", description, ImageSources.Error);


    private void AddGameButton_Click(object sender, RoutedEventArgs e)
    {   
        InputDialog addGameDialog = new("Add Game", "Enter the name of the new game:");

        while (addGameDialog.ShowDialog(this) == true)
        {
            try
            {
                GameEditViewModel.AddGame(addGameDialog.Input);
                break;
            }
            catch (ValidationException ex)
            {
                new OkDialog("Invalid name", ex.Message, ImageSources.Warning).ShowDialog(this);
                addGameDialog = new(addGameDialog.Title, addGameDialog.Prompt, addGameDialog.Input);
            }                                  
        }            
    }


    private void RemoveGameButton_Click(object sender, RoutedEventArgs e)
    {
        if (GameEditViewModel.ActiveGame == null)
        {
            return;
        }

        YesNoDialog confirmationDialog = new("Remove Game",
            $"Are you sure you want to remove '{GameEditViewModel.ActiveGame.Name}'?\nIts profiles will not be deleted.");

        if (confirmationDialog.ShowDialog(this) == true)
        {
            GameEditViewModel.RemoveGame();
        }
    }


    private void RenameGameButton_Click(object sender, RoutedEventArgs e)
    {
        if (GameEditViewModel.ActiveGame == null)
        {
            return;
        }

        InputDialog renameGameDialog = new("Rename Game", "Enter the new name of the game:", GameEditViewModel.ActiveGame.Name);

        while (renameGameDialog.ShowDialog(this) == true)
        {
            try
            {
                GameEditViewModel.RenameGame(renameGameDialog.Input);
                break;
            }
            catch (ValidationException ex)
            {
                new OkDialog("Invalid name", ex.Message, ImageSources.Warning).ShowDialog(this);
                renameGameDialog = new(renameGameDialog.Title, renameGameDialog.Prompt, renameGameDialog.Input);
            }                                  
        }   
    }


    private void SavefileBrowseButton_Click(object sender, RoutedEventArgs e)
    {
        if (GameEditViewModel.ActiveGame == null)
        {
            return;
        }

        CommonOpenFileDialog openFileDialog = new()
        {
            IsFolderPicker = false,
            Title = "Select a savefile",
            InitialDirectory = Path.GetDirectoryName(GameEditViewModel.ActiveGame.SavefileLocation)            
        };

        if (openFileDialog.ShowDialog(this) == CommonFileDialogResult.Ok)
        {
            try
            {
                GameEditViewModel.SetSavefileLocation(openFileDialog.FileName);
            }
            catch (ValidationException ex)
            {
                new OkDialog("Invalid file", ex.Message, ImageSources.Warning).ShowDialog(this);
            }
            catch (FilesystemException)
            {
                CreateErrorDialog("An error occurred while setting the savefile location.").ShowDialog(this);
            }
        }
    }


    private void SavefileResetButton_Click(object sender, RoutedEventArgs e)
    {
        GameEditViewModel.SetSavefileLocation(null);
    }


    private void ProfilesDirectoryBrowseButton_Click(object sender, RoutedEventArgs e)
    {
        if (GameEditViewModel.ActiveGame == null)
        {
            return;
        }

        CommonOpenFileDialog openFolderDialog = new()
        {
            IsFolderPicker = true,
            Title = "Select a profiles directory",
            InitialDirectory = GameEditViewModel.ActiveGame?.ProfilesDirectory
        };

        if (openFolderDialog.ShowDialog(this) == CommonFileDialogResult.Ok)
        {
            try
            {
                GameEditViewModel.SetProfilesDirectory(openFolderDialog.FileName);
            }
            catch (ValidationException ex)
            {
                new OkDialog("Invalid directory", ex.Message, ImageSources.Warning).ShowDialog(this);
            }
            catch (FilesystemException)
            {
                CreateErrorDialog("An error occurred while setting the profiles directory.").ShowDialog(this);
            }
        }
    }


    private void ProfilesDirectoryResetButton_Click(object sender, RoutedEventArgs e)
    {
        GameEditViewModel.SetProfilesDirectory(null);
    }


    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        bool shouldAttemptSave = true;

        while (shouldAttemptSave)
        {      
            try
            {
                GameEditViewModel.SaveGamesToAppdata();
                shouldAttemptSave = false;
            }
            catch (AppdataException)
            {
                YesNoDialog tryAgainDialog = new("Failed to save",
                    "Failed to save changes to games (appdata.xml might be in use by another program)\nWould you like to try again?",
                    ImageSources.Error);

                if (tryAgainDialog.ShowDialog(this) != true)
                {
                    e.Cancel = true;
                    shouldAttemptSave = false;
                }
            }
        }
    }
}
