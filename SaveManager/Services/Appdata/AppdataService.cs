using SaveManager.Exceptions;
using SaveManager.Models;
using System.IO;
using System.Text.Json;

namespace SaveManager.Services.Appdata;

public class AppdataService : IAppdataService
{
    private const string AppdataDirectoryName = "Save Manager";
    private static readonly string AppdataDirectory;
    private static readonly string AppdataLocation;

    private Appdata _appdata = new();


    static AppdataService()
    {
        string basePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppdataDirectoryName);

        #if DEBUG
            AppdataDirectory = Path.Join(basePath, "Debug");
        #else
            AppdataDirectory = Path.Join(basePath, "Release");
        #endif

        AppdataLocation = Path.Join(AppdataDirectory, "appdata.json");        
    }


    /// <summary>
    /// Initializes the AppdataService for use.
    /// </summary>
    public AppdataService()
    {
        Initialize();
    }




    /// <summary>
    /// Loads the appdata file if it exists. Ensures the appdata folder exists.
    /// </summary>
    /// <exception cref="AppdataException"></exception>
    internal void Initialize()
    {
        try
        {
            if (!Directory.Exists(AppdataDirectory))
            {
                Directory.CreateDirectory(AppdataDirectory);
            }

            if (Path.Exists(AppdataLocation))
            {
                string jsonString = File.ReadAllText(AppdataLocation);
                _appdata = JsonSerializer.Deserialize<Appdata>(jsonString)!;
            }
        }
        catch (Exception ex)
        {
            if (ex is IOException or UnauthorizedAccessException or ArgumentException or PathTooLongException or
                DirectoryNotFoundException or NotSupportedException or FileNotFoundException or System.Security.SecurityException)
            {
                throw new AppdataException("Unable to load appdata.", ex);
            }
            else if (ex is JsonException)
            {
                throw new AppdataException($"Appdata contains invalid data. Delete it and launch again:\n{AppdataLocation}", ex);
            }
            
            throw;
        }    
    }


    /// <summary>
    /// Overwrites the appdata file with the current data.
    /// </summary>
    /// <exception cref="AppdataException"></exception>
    public void SaveAppdata()
    {
        string jsonString = JsonSerializer.Serialize(_appdata);
        try
        {            
            File.WriteAllText(AppdataLocation, jsonString);
        }
        catch (Exception ex)
        {
            if (ex is ArgumentException or PathTooLongException or DirectoryNotFoundException or IOException or
                UnauthorizedAccessException or NotSupportedException or System.Security.SecurityException)
            {
                throw new AppdataException(ex.Message, ex);
            }
                
            throw;
        }        
    }


    /// <summary>
    /// Sets the game data in the internal appdata representation.
    /// </summary>
    /// <param name="games"></param>
    public void SetGameData(IEnumerable<Game> games)
    {
        _appdata.Games = games.Select(ConvertToGameDTO).OrderBy(x => x.Name);
    }


    /// <summary>
    /// Retrieves game data ordered by name.
    /// </summary>
    /// <returns>A list of <see cref="GameDTO"/>'s</returns>
    public IEnumerable<GameDTO> GetGameData()
    {
        return _appdata.Games;
    }


    /// <summary>
    /// Sets the startup preferences in the internal appdata representation.
    /// </summary>
    /// <param name="startupPreferences"></param>
    public void SetStartupPreferences(StartupPreferences startupPreferences)
    {
        _appdata.StartupPreferences = startupPreferences;
    }


    /// <summary>
    /// Gets the startup preferences from the internal appdata representation.
    /// </summary>
    /// <returns></returns>
    public StartupPreferences GetStartupPreferences()
    {
        return _appdata.StartupPreferences;
    }


    private static GameDTO ConvertToGameDTO(Game game)
    {
        return new()
        {
            Name = game.Name,
            ProfilesDirectory = game.ProfilesDirectory,
            SavefileLocation = game.SavefileLocation
        };
    }
}
