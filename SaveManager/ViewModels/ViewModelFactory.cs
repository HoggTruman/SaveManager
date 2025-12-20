using SaveManager.Models;
using SaveManager.Services.Appdata;
using SaveManager.Services.Hotkey;

namespace SaveManager.ViewModels;

public static class ViewModelFactory
{
    private static IAppdataService? _appdataService;

    /// <summary>
    /// Sets up ViewModelFactory with the dependencies needed to create view models.
    /// </summary>
    /// <param name="appdataService"></param>
    public static void Initialize(IAppdataService appdataService)
    {
        _appdataService = appdataService;
    }

    /// <summary>
    /// Creates a new SaveViewModel instance with its dependencies.
    /// </summary>
    /// <param name="games"></param>
    /// <returns>A SaveViewModel.</returns>
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
    /// <returns>A GameEditViewModel.</returns>
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


    /// <summary>
    /// Creates a new SettingsViewModel instance with its dependencies.
    /// </summary>
    /// <param name="hotkeyService">The hotkey service for the window hotkeys will be registered to.</param>
    /// <returns>A SettingsViewModel.</returns>
    public static SettingsViewModel CreateSettingsViewModel(IHotkeyService hotkeyService)
    {
        if (_appdataService == null)
        {
            throw new InvalidOperationException("ViewModelFactory has not been initialized.");
        }

        return new SettingsViewModel(hotkeyService, _appdataService);
    }
}
