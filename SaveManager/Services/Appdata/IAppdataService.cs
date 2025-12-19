using SaveManager.Models;
using SaveManager.Exceptions;

namespace SaveManager.Services.Appdata;

public interface IAppdataService
{
    /// <summary>
    /// Overwrites the appdata file with the current data.
    /// </summary>
    /// <exception cref="AppdataException"></exception>
    public void SaveAppdata();

    /// <summary>
    /// Sets the game data in the internal appdata representation.
    /// </summary>
    /// <param name="games">A list of games.</param>
    public void SetGameData(IEnumerable<Game> games);

    /// <summary>
    /// Retrieves game data ordered by name.
    /// </summary>
    /// <returns>A list of <see cref="GameDTO"/>'s</returns>
    public IEnumerable<GameDTO> GetGameData();

    /// <summary>
    /// Sets the startup preferences in the internal appdata representation.
    /// </summary>
    /// <param name="startupPreferences">The startup preferences object to set.</param>
    public void SetStartupPreferences(StartupPreferences startupPreferences);

    /// <summary>
    /// Gets the startup preferences from the internal appdata representation.
    /// </summary>
    /// <returns>A startup preferences object.</returns>
    public StartupPreferences GetStartupPreferences();

    /// <summary>
    /// Sets the settings in the internal appdata representation.
    /// </summary>
    /// <param name="settings">The Settings object to set.</param>
    public void SetSettings(Settings settings);

    /// <summary>
    /// Gets the settings from the internal appdata representation.
    /// </summary>
    /// <returns>A Settings object.</returns>
    public Settings GetSettings();
}
