using SaveManager.DTOs;
using System.IO;

namespace SaveManager.Models;

public class Game
{
    public required string Name { get; set; }
    public string? SavefileLocation { get; set; }
    public string? ProfilesDirectory { get; set; }

    public List<Profile> Profiles { get; set; } = [];


    /// <summary>
    /// Loads the games profiles from the filesystem.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException"/>
    /// <exception cref="System.Security.SecurityException"/>
    public void LoadProfiles()
    {
        Profiles = ProfilesDirectory != null && Directory.Exists(ProfilesDirectory)?
            [..Directory.EnumerateDirectories(ProfilesDirectory).Select(x => new Profile(x))]:
            [];
    }
}
