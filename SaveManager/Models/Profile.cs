using SaveManager.Exceptions;

namespace SaveManager.Models;

public class Profile
{
    public Game Game { get; }
    public Folder Folder { get; }

    public string Name => Folder.Name;


    /// <summary>
    /// Initializes a new <see cref="Profile"/> instance.
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="game"></param>
    public Profile(Folder folder, Game game)
    {
        Folder = folder;
        Game = game;
    }


    /// <summary>
    /// Creates a new profile in the provided game's profiles directory.
    /// Returns a <see cref="Profile"/> instance representing it.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="game"></param>
    /// <exception cref="ValidationException"></exception>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="FilesystemItemNotFoundException"></exception>
    public static Profile Create(string name, Game game)
    {
        if (game.ProfilesFolder == null)
        {
            throw new InvalidOperationException("ProfilesDirectory must be set before a profile can be created.");
        }

        if (game.Profiles.Any(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)))
        {
            throw new ValidationException($"A profile already exists with this name.");
        }

        Folder profileFolder = Folder.Create(name, game.ProfilesFolder);
        Profile profile = new(profileFolder, game);
        game.Profiles.Add(profile);
        game.SortProfiles();
        return profile;
    }


    /// <summary>
    /// Renames the Profile.
    /// </summary>
    /// <param name="newName"></param>
    /// <exception cref="ValidationException"></exception>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="FilesystemItemNotFoundException"></exception>
    public void Rename(string newName)
    {
        if (Game.Profiles.Any(x => x.Name.Equals(newName, StringComparison.CurrentCultureIgnoreCase)))
        {
            throw new ValidationException("A Profile already exists with this name");
        }

        Folder.Rename(newName);
        Game.SortProfiles();
    }


    /// <summary>
    /// Deletes the <see cref="Profile"/>.
    /// </summary>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="FilesystemItemNotFoundException"></exception>
    public void Delete()
    {
        Folder.Delete();
        Game.Profiles = [..Game.Profiles.Where(x => x != this)];
    }


    public override string ToString()
    {
        return Name;
    }
}
