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

    public static SaveViewModel CreateSaveViewModel(IEnumerable<Game> games)
    {
        if (_appdataService == null)
        {
            throw new InvalidOperationException("ViewModelFactory has not been initialized.");
        }

        return new(_appdataService, games);
    }

    public static GameEditViewModel CreateGameEditViewModel(IEnumerable<Game> games, Game? activeGame)
    {
        if (_appdataService == null)
        {
            throw new InvalidOperationException("ViewModelFactory has not been initialized.");
        }

        return new(_appdataService, games, activeGame);
    }
}
