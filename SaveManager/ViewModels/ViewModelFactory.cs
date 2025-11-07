using SaveManager.Models;
using SaveManager.Services.Appdata;

namespace SaveManager.ViewModels;

public static class ViewModelFactory
{
    private static AppdataService? _appdataService;

    /// <summary>
    /// Sets up ViewModelFactory with the dependencies needed to create view models.
    /// </summary>
    /// <param name="appdataService"></param>
    public static void Initialize(AppdataService appdataService)
    {
        _appdataService = appdataService;
    }

    /// <summary>
    /// Creates a new SaveViewModel instance with its dependencies.
    /// </summary>
    /// <param name="games"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static SaveViewModel CreateSaveViewModel(IEnumerable<Game> games)
    {
        if (_appdataService == null)
        {
            throw new InvalidOperationException("ViewModelFactory has not been initialized.");
        }

        return new SaveViewModel(_appdataService)
        {
            Games = [..games],
            ActiveGame = games.FirstOrDefault()
        };
    }


    /// <summary>
    /// Creates a new GameEditViewModel instance with its dependencies.
    /// </summary>
    /// <param name="games"></param>
    /// <param name="activeGame"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static GameEditViewModel CreateGameEditViewModel(IEnumerable<Game> games, Game? activeGame)
    {
        if (_appdataService == null)
        {
            throw new InvalidOperationException("ViewModelFactory has not been initialized.");
        }

        return new GameEditViewModel(_appdataService)
        {
            Games = [..games],
            ActiveGame = activeGame
        };
    }
}
