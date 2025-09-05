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
            List<Game> games = [];
            int invalidGameCount = 0;

            foreach (GameDTO gameDTO in appdataService.GetGameData())
            {
                try
                {
                    // When games are constructed, if the profiles directory is inaccessible, the field is set to null.
                    Game game = new(gameDTO.Name, gameDTO.SavefileLocation, gameDTO.ProfilesDirectory);
                    games.Add(game);
                }
                catch (ValidationException)
                {
                    // Invalid game name retrieved from appdata.xml
                    ++invalidGameCount;
                }
            }

            // Save games once they have been loaded to remove any invalid/outdated data.
            appdataService.ReplaceGames(games); 

            if (invalidGameCount > 0)
                MessageBox.Show($"{invalidGameCount} games removed due to invalid data.");

            SaveWindow mainWindow = new(ViewModelFactory.CreateSaveViewModel(games));
            mainWindow.Show();  
        }
        catch (AppdataException ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
}
