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
    /// Renames the Profile.
    /// </summary>
    /// <param name="newName"></param>
    /// <exception cref="ValidationException"></exception>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="FileAccessException"></exception>
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
    /// <exception cref="FileAccessException"></exception>
    public void Delete()
    {
        Folder.Delete();
        Game.Profiles = [..Game.Profiles.Where(x => x != this)];
    }
}
