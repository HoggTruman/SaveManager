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
    /// <param name="games"></param>
    public void SetGameData(IEnumerable<Game> games);

    /// <summary>
    /// Retrieves game data ordered by name.
    /// </summary>
    /// <returns>A list of <see cref="GameDTO"/>'s</returns>
    public IEnumerable<GameDTO> GetGameData();

    /// <summary>
    /// Sets the startup preferences in the internal appdata representation.
    /// </summary>
    /// <param name="startupPreferences"></param>
    public void SetStartupPreferences(StartupPreferences startupPreferences);

    /// <summary>
    /// Gets the startup preferences from the internal appdata representation.
    /// </summary>
    /// <returns></returns>
    public StartupPreferences GetStartupPreferences();
}
