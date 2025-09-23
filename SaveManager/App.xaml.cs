using SaveManager.DTOs;
using SaveManager.Exceptions;
using SaveManager.Models;
using SaveManager.Services;
using SaveManager.ViewModels;
using SaveManager.Views.Save;
using System.Windows;

namespace SaveManager;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            AppdataService appdataService = new();
            ViewModelFactory.Initialize(appdataService);
            List<Game> games = LoadGames(appdataService);            

            // Save games once they have been loaded to remove any invalid/outdated data.
            appdataService.ReplaceGames(games);
            SaveWindow mainWindow = new(ViewModelFactory.CreateSaveViewModel(games));
            mainWindow.Show();  
        }
        catch (FilesystemException)
        {
            MessageBox.Show("Failed to initialize app.");
        }
        catch (AppdataException ex)
        {
            MessageBox.Show(ex.Message);
        }        
    }

    /// <summary>
    /// Retrieves the user's games from the appdata service and loads their profiles from the filesystem.
    /// </summary>
    /// <param name="appdataService"></param>
    /// <returns>A list of loaded games</returns>
    /// <exception cref="FilesystemException"></exception>
    internal List<Game> LoadGames(AppdataService appdataService)
    {
        List<Game> games = [];
        int invalidGameCount = 0;

        foreach (GameDTO gameDTO in appdataService.GetGameData())
        {
            try
            {
                Game game = new(gameDTO.Name) { SavefileLocation = gameDTO.SavefileLocation };
                try
                {
                    game.ProfilesDirectory = gameDTO.ProfilesDirectory;
                    games.Add(game);
                }
                catch (FilesystemItemNotFoundException)
                {
                    // Profiles directory no longer exists
                    game.ProfilesDirectory = null; 
                }                
            }
            catch (ValidationException)
            {
                // Invalid game name retrieved from appdata.xml
                ++invalidGameCount;
            }
        }

        if (invalidGameCount > 0)
                MessageBox.Show($"{invalidGameCount} games removed due to invalid data.");

        return games;
    }
}
