using SaveManager.Exceptions;
using SaveManager.Models;
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
        
        // When games are constructed, if the profiles directory is inaccessible, the field is reset.
        // Save games on startup to make sure appdata is cleared off any bad data.
        // catch is for if bad data is present due to tampering with the appdata file.
        try
        {
            IEnumerable<Game> games = ViewModelFactory.AppdataService.GetGames();
            ViewModelFactory.AppdataService.ReplaceGames(games);
            SaveWindow mainWindow = new(ViewModelFactory.CreateSaveViewModel(games));
            mainWindow.Show();   
        }
        catch (AppdataException ex)
        {
            MessageBox.Show(ex.Message);
            return;
        }      
    }
}
